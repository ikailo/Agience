using System.Reflection;
using NuGet.Versioning;
using NuGet.Common;
using NuGet.Packaging.Core;
using NuGet.Packaging;
using NuGet.Protocol.Core.Types;
using NuGet.Protocol;
using NuGet.Configuration;
using NuGet.Packaging.Signing;
using NuGet.Resolver;
using NuGet.Frameworks;

namespace Agience.Core.Extensions
{
    internal static class NuGetExtensions
    {

        /// <summary>
        /// Checks that the exceptions type is about a missing Nuget dependency.
        /// </summary>
        /// <param name="ex"></param>
        internal static bool IsMissingNugetException(this Exception ex)
        {
            return ex is ReflectionTypeLoadException && ex.Message.Contains("Could not load file or assembly");
        }

        /// <summary>
        /// Captures the package info in the exception details from a missing Nuget dependency needed.
        /// </summary>
        /// <param name="ex"></param>
        internal static (string PackageName, string Version) GetNugetPackageInfo(this Exception ex)
        {
            var errorIndexText = "Could not load file or assembly '";
            var messagePartOne = ex.Message.Substring(ex.Message.IndexOf(errorIndexText) + errorIndexText.Length);
            var packageName = messagePartOne.Substring(0, messagePartOne.IndexOf(","));
            var versionIndexText = "Version=";
            var messagePartTwo = messagePartOne.Substring(messagePartOne.IndexOf(versionIndexText) + versionIndexText.Length);
            var version = messagePartTwo.Substring(0, messagePartTwo.IndexOf(","));
            return (packageName, version);
        }

        internal static async Task InstallNugetPackage(string packageName, string version)
        {
            try
            {
                ILogger logger = NullLogger.Instance;
                CancellationToken cancellationToken = CancellationToken.None;

                SourceCacheContext cache = new SourceCacheContext();
                SourceRepository repository = Repository.Factory.GetCoreV3("https://api.nuget.org/v3/index.json");

                var targetFramework = $"net{Environment.Version.Major}.0";

                // Find all potential dependencies
                var packages = new HashSet<SourcePackageDependencyInfo>(PackageIdentityComparer.Default);
                await listAllPackageDependencies(
                    new PackageIdentity(packageName, NuGetVersion.Parse(version)),
                    repository,
                    NuGetFramework.ParseFolder(targetFramework),
                    cache,
                    logger,
                    packages,
                    cancellationToken);

                // Find the best version for each package
                var resolverContext = new PackageResolverContext(
                    dependencyBehavior: DependencyBehavior.Lowest,
                    targetIds: new[] { packageName },
                    requiredPackageIds: Enumerable.Empty<string>(),
                    packagesConfig: Enumerable.Empty<PackageReference>(),
                    preferredVersions: Enumerable.Empty<PackageIdentity>(),
                    availablePackages: packages,
                    new[] { repository.PackageSource },
                    NullLogger.Instance);

                var resolver = new PackageResolver();
                var resolvedPackages = resolver.Resolve(resolverContext, CancellationToken.None);

                foreach (var resolvedPackage in resolvedPackages)
                {
                    //Optionally it is possible to load package dependencies here, if needed
                    //await loadAssemblyPackage(resolvedPackage.Id, resolvedPackage.version, installationPath, repository, cache, logger, cancellationToken);
                }

                //Load Assembly Package
                await loadAssemblyPackage(packageName, version, repository, cache, logger, cancellationToken);

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private static async Task loadAssemblyPackage(
            string packageName,
            string version,
            SourceRepository repository,
            SourceCacheContext cache,
            ILogger logger,
            CancellationToken cancellationToken
            )
        {
            try
            {
                FindPackageByIdResource findPackageByIdResource = await repository.GetResourceAsync<FindPackageByIdResource>();
                NuGetVersion packageVersion = new NuGetVersion(version);

                //Add it to the Local Environment Nuget package folder   
                using MemoryStream packageStream = new MemoryStream();
                await findPackageByIdResource.CopyNupkgToStreamAsync(packageName, packageVersion, packageStream, cache, logger, cancellationToken);
                packageStream.Seek(0, SeekOrigin.Begin);
                var settings = Settings.LoadDefaultSettings(null);
                var globalPackagesFolder = SettingsUtility.GetGlobalPackagesFolder(settings);
                var downloadResult = await GlobalPackagesFolderUtility.AddPackageAsync(
                    "https://api.nuget.org/v3/index.json",
                    new PackageIdentity(packageName, packageVersion),
                    packageStream,
                    globalPackagesFolder,
                    parentId: Guid.Empty,
                    ClientPolicyContext.GetClientPolicy(settings, logger),
                    logger,
                    cancellationToken);

                //Find the best Framework version
                var baseNugetLibPath = Path.GetDirectoryName(((FileStream)downloadResult.PackageStream).Name) + "\\lib";
                baseNugetLibPath = getBestFrameworkNugetLibPath(baseNugetLibPath);

                //Load assembly into the running App
                Assembly.LoadFrom(baseNugetLibPath + "\\" + packageName + ".dll");
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        /// <summary>
        /// Get the best framework library from the Nuget folder.
        /// </summary>
        /// <param name="basePath"></param>
        private static string getBestFrameworkNugetLibPath(string basePath)
        {

            //TODO: Optimize to detect the preferred running app library framework without hardcoding

            if (Directory.Exists(basePath + $"\\net8.0"))
                return basePath + $"\\net8.0";

            if (Directory.Exists(basePath + $"\\net7.0"))
                return basePath + $"\\net7.0";

            if (Directory.Exists(basePath + $"\\net6.0"))
                return basePath + $"\\net6.0";

            if (Directory.Exists(basePath + $"\\netstandard2.1"))
                return basePath + $"\\netstandard2.1";

            if (Directory.Exists(basePath + $"\\netstandard2.0"))
                return basePath + $"\\netstandard2.0";

            if (Directory.Exists(basePath + $"\\netstandard1.1"))
                return basePath + $"\\netstandard1.1";

            throw new Exception("Nuget Lib folder for a compatible Net Framework not found.");
        }

        private static async Task listAllPackageDependencies(
            PackageIdentity package,
            SourceRepository repository,
            NuGetFramework targetFramework,
            SourceCacheContext cache,
            ILogger logger,
            HashSet<SourcePackageDependencyInfo> dependencies,
            CancellationToken cancellationToken)
        {
            if (dependencies.Contains(package))
                return;

            var dependencyInfoResource = await repository.GetResourceAsync<DependencyInfoResource>();
            var dependencyInfo = await dependencyInfoResource.ResolvePackage(package, targetFramework, cache, logger, cancellationToken);

            if (dependencyInfo == null)
                return;

            if (dependencies.Add(dependencyInfo))
            {
                foreach (var dependency in dependencyInfo.Dependencies)
                {
                    await listAllPackageDependencies(
                        new PackageIdentity(dependency.Id, dependency.VersionRange.MinVersion),
                        repository,
                        targetFramework,
                        cache,
                        logger,
                        dependencies,
                        cancellationToken);
                }
            }
        }

    }
}