using System.Collections.Concurrent;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography;
using System.Text.Json;
using Agience.Authority.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Agience.Core.Extensions;

namespace Agience.Authority.Identity.Services
{
    public class PluginImportService
    {
        /*
        private readonly ILogger<PluginImportService> _logger;
        private readonly string _workspacePath;

        // In-memory storage for import statuses
        private readonly ConcurrentDictionary<string, string> _importStatuses = new();

        public PluginImportService(ILogger<PluginImportService> logger, AppConfig appConfig)
        {
            _logger = logger;
            _workspacePath = appConfig.WorkspacePath ?? throw new ArgumentNullException(nameof(appConfig.WorkspacePath));
        }

        #region Import Process Management

        public string StartRepoImport(string repoUrl, Action<string, PluginLibrary> onComplete)
        {
            var importId = CreateImport("InProgress");

            Task.Run(async () =>
            {
                try
                {
                    var pluginLibrary = await ProcessRepositoryAsync(repoUrl);
                    onComplete(importId, pluginLibrary); // Caller handles saving
                    UpdateImportStatus(importId, "Completed");
                }
                catch (Exception ex)
                {
                    HandleImportFailure(importId, ex);
                }
            });

            return importId;
        }

        public Task<string> StartDirectoryImport(string directoryPath, Action<string, PluginLibrary> onComplete)
        {
            var importId = CreateImport("InProgress");

            // Run the analysis in a background task
            Task.Run(async () =>
            {
                try
                {
                    var pluginLibrary = await AnalyzePrebuiltDirectory(directoryPath);
                    onComplete(importId, pluginLibrary); // Caller handles saving
                    UpdateImportStatus(importId, "Completed");
                }
                catch (Exception ex)
                {
                    HandleImportFailure(importId, ex);
                }
            });

            return Task.FromResult(importId);
        }



        public string GetImportStatus(string importId)
        {
            return _importStatuses.TryGetValue(importId, out var status) ? status : "NotFound";
        }

        #endregion

        #region Import Logic

        private async Task<PluginLibrary> ProcessRepositoryAsync(string repoUrl)
        {
            string tempDirectory = CreateTempDirectory();
            string buildDirectory = CreateBuildDirectory(tempDirectory);

            try
            {
                string repoPath = ExtractRepositoryPath(repoUrl);
                string csprojPath = ExtractCsprojPath(repoUrl);

                CloneRepository(repoPath, tempDirectory);
                BuildProject(Path.Combine(tempDirectory, csprojPath), buildDirectory);

                var dllFiles = CollectDllFiles(buildDirectory);
                return await AnalyzeDlls(dllFiles);
            }
            finally
            {
                Cleanup(tempDirectory, buildDirectory);
            }
        }

        private async Task<PluginLibrary> AnalyzePrebuiltDirectory(string directoryPath)
        {
            if (!Directory.Exists(directoryPath))
                throw new DirectoryNotFoundException($"Directory not found: {directoryPath}");

            _logger.LogInformation("Analyzing prebuilt directory: {Directory}", directoryPath);

            // Collect DLLs from the provided directory
            var dllFiles = Directory.GetFiles(directoryPath, "*.dll", SearchOption.AllDirectories);
            if (!dllFiles.Any())
                throw new FileNotFoundException("No DLL files were found in the provided directory.");

            return await AnalyzeDlls(dllFiles);
        }

        #endregion

        #region Helper Methods

        private string CreateImport(string status)
        {
            var importId = Guid.NewGuid().ToString();
            _importStatuses[importId] = status;
            return importId;
        }

        private void HandleImportFailure(string importId, Exception ex)
        {
            _logger.LogError(ex, "Import process failed.");
            UpdateImportStatus(importId, "Failed");
        }

        private void UpdateImportStatus(string importId, string status)
        {
            _importStatuses[importId] = status;
        }

        private string CreateTempDirectory() =>
            Path.Combine(_workspacePath, "temp", Guid.NewGuid().ToString());

        private string CreateBuildDirectory(string basePath) =>
            Path.Combine(basePath, "build");

        private void CloneRepository(string repoUrl, string destination)
        {
            ExecuteCommand($"git clone {repoUrl} {destination}");
        }

        private void BuildProject(string projectPath, string outputDirectory)
        {
            string dockerImage = "mcr.microsoft.com/dotnet/sdk:6.0";
            string projectDir = Path.GetDirectoryName(projectPath);
            string buildCommand = $"run --rm -v \"{projectDir}:/src\" -v \"{outputDirectory}:/out\" -w /src {dockerImage} dotnet build -o /out";

            ExecuteCommand($"docker {buildCommand}");
        }

        private IEnumerable<string> CollectDllFiles(string buildDirectory)
        {
            var dllFiles = Directory.GetFiles(buildDirectory, "*.dll", SearchOption.AllDirectories);

            if (!dllFiles.Any())
                throw new FileNotFoundException("No DLL files were found after building.");

            return dllFiles;
        }

        private async Task<PluginLibrary> AnalyzeDlls(IEnumerable<string> dllFiles)
        {
            var pluginLibrary = new PluginLibrary();
            var assemblyLoadContext = new Dictionary<string, Assembly>();

            // Load assemblies with dependency resolution
            foreach (var dll in dllFiles)
            {
                try
                {
                    if (!assemblyLoadContext.ContainsKey(dll))
                    {
                        var assembly = await LoadAssemblyWithDependencies(dll, dllFiles, assemblyLoadContext);
                        assemblyLoadContext[dll] = assembly;

                        foreach (var type in assembly.GetTypes())
                        {
                            var methods = type.GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                                .Where(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), false).Any());

                            if (methods.Any())
                            {
                                pluginLibrary.Plugins.Add(new Plugin
                                {
                                    Name = type.Name,
                                    Functions = methods.Select(m => new Function { Name = m.Name }).ToList()
                                });
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to load or analyze assembly: {Dll}", dll);
                }
            }

            _logger.LogInformation("Analysis complete. Found {PluginCount} plugins.", pluginLibrary.Plugins.Count);
            return pluginLibrary;
        }


        private async Task<Assembly> LoadAssemblyWithDependencies(string dllPath, IEnumerable<string> allDllFiles, Dictionary<string, Assembly> assemblyLoadContext)
        {
            try
            {
                var assembly = Assembly.LoadFile(dllPath);
                return assembly;
            }
            catch (ReflectionTypeLoadException ex) when (ex.IsMissingNugetException())
            {
                // Resolve missing NuGet dependency and retry
                var packageInfo = ex.GetNugetPackageInfo();
                await NuGetExtensions.InstallNugetPackage(packageInfo.PackageName, packageInfo.Version);

                // Retry loading after resolving the dependency
                return Assembly.LoadFile(dllPath);
            }
            catch (FileNotFoundException ex)
            {
                // Look for the missing dependency in the provided DLLs
                var missingAssemblyName = ex.Message.Split('\'')[1];
                var dependencyPath = allDllFiles.FirstOrDefault(file =>
                    Path.GetFileNameWithoutExtension(file).Equals(missingAssemblyName, StringComparison.OrdinalIgnoreCase));

                if (!string.IsNullOrEmpty(dependencyPath) && !assemblyLoadContext.ContainsKey(dependencyPath))
                {
                    var dependencyAssembly = await LoadAssemblyWithDependencies(dependencyPath, allDllFiles, assemblyLoadContext);
                    assemblyLoadContext[dependencyPath] = dependencyAssembly;

                    // Retry loading the original assembly
                    return Assembly.LoadFile(dllPath);
                }

                throw; // Re-throw if the dependency cannot be resolved
            }
        }


        private Dictionary<string, List<string>> BuildDependencyGraph(IEnumerable<string> dllFiles)
        {
            var dependencyGraph = new Dictionary<string, List<string>>();

            foreach (var dll in dllFiles)
            {
                try
                {
                    var assemblyName = AssemblyName.GetAssemblyName(dll);
                    var assembly = Assembly.LoadFile(dll);
                    var referencedAssemblies = assembly.GetReferencedAssemblies();

                    dependencyGraph[dll] = referencedAssemblies
                        .Select(a => dllFiles.FirstOrDefault(f => Path.GetFileNameWithoutExtension(f) == a.Name))
                        .Where(refDll => refDll != null)
                        .ToList()!;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to read dependencies for assembly: {Dll}", dll);
                    dependencyGraph[dll] = new List<string>();
                }
            }

            return dependencyGraph;
        }

        private List<string> TopologicalSort(IEnumerable<string> dllFiles, Dictionary<string, List<string>> dependencyGraph)
        {
            var sorted = new List<string>();
            var visited = new HashSet<string>();

            void Visit(string dll)
            {
                if (!visited.Contains(dll))
                {
                    visited.Add(dll);

                    if (dependencyGraph.TryGetValue(dll, out var dependencies))
                    {
                        foreach (var dependency in dependencies)
                        {
                            Visit(dependency);
                        }
                    }

                    sorted.Add(dll);
                }
            }

            foreach (var dll in dllFiles)
            {
                Visit(dll);
            }

            return sorted;
        }


        private void ExecuteCommand(string command)
        {
            var processInfo = new ProcessStartInfo
            {
                FileName = "bash",
                Arguments = $"-c \"{command}\"",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(processInfo);
            process.WaitForExit();

            if (process.ExitCode != 0)
                throw new Exception($"Command execution failed: {process.StandardError.ReadToEnd()}");
        }

        private void Cleanup(params string[] paths)
        {
            foreach (var path in paths.Where(Directory.Exists))
            {
                try
                {
                    Directory.Delete(path, true);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to delete directory: {Path}", path);
                }
            }
        }

        private string ExtractRepositoryPath(string repoUrl) =>
            $"https://github.com/{repoUrl.Split('/')[3]}/{repoUrl.Split('/')[4]}.git";

        private string ExtractCsprojPath(string repoUrl) =>
            repoUrl.Split(new[] { "/blob/main/" }, StringSplitOptions.None).LastOrDefault()?.Replace(Path.GetFileName(repoUrl), "") ?? throw new ArgumentException("Invalid repo URL");

        #endregion
        */
    }
}
