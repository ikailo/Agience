using Agience.Plugins.Core.System;
using LibGit2Sharp;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;

namespace Agience.Plugins.Core.Code
{
    public class Git
    {
        private string _workingDirectory;

        private Files _files;

        public Git(string workingDirectory)
        {
            _workingDirectory = workingDirectory;
            _files = new Files(workingDirectory);
        }

        [KernelFunction, Description("Clone a repository into the working directory.")]
        [return: Description("The path to the cloned repository relative to the working directory.")]
        public Task<string> CloneRepositoryAsync(
            [Description("The uri of the repository.")] string repositoryUri
            )
        {
            var clonePathRelative = Path.GetFileName(repositoryUri);

            if (clonePathRelative.EndsWith(".git"))
            {
                clonePathRelative = clonePathRelative.Substring(0, clonePathRelative.Length - 4);
            }

            var clonePath = Path.Combine(_workingDirectory, clonePathRelative);

            Repository.Clone(repositoryUri, clonePath);

            return Task.FromResult(clonePath);
        }

        [KernelFunction, Description("Create a file that lists the repository files, while respecting gitignore filtering.")]
        [return: Description("The filename of the line-delimited file containing filtered list of files in the repository, relative to the working directory")]
        public async Task<string> WriteFileListGitAware(
             [Description("The root path for the repository.")] string rootDirectory,
             [Description("The filename to write the list to.")] string filename

         )
        {
            var fileListBuilder = new StringBuilder();

            using (var repo = new Repository(rootDirectory))
            {
                var ignoredRules = repo.Ignore;

                foreach (var file in Directory.EnumerateFiles(rootDirectory, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(rootDirectory, file);

                    if (!ignoredRules.IsPathIgnored(relativePath))
                    {
                        fileListBuilder.AppendLine(relativePath);
                    }
                }
            }
            await _files.WriteTextToFile(filename, fileListBuilder.ToString());

            return filename;
        }

        [KernelFunction, Description("Create a file that lists the repository directories, while respecting gitignore filtering.")]
        [return: Description("The filename of the line-delimited file containing filtered list of directories in the repository, relative to the working directory")]
        public async Task<string> WriteDirectoryListGitAware(
         [Description("The root path for the repository.")] string rootDirectory,
         [Description("The filename to write the list to.")] string filename
 )
        {
            var directoryListBuilder = new StringBuilder();

            using (var repo = new Repository(rootDirectory))
            {
                var ignoredRules = repo.Ignore;

                foreach (var directory in Directory.EnumerateDirectories(rootDirectory, "*", SearchOption.AllDirectories))
                {
                    var relativePath = Path.GetRelativePath(rootDirectory, directory);

                    // Check if any file within this directory is not ignored
                    bool hasVisibleFiles = Directory.EnumerateFiles(directory)
                        .Any(file => !ignoredRules.IsPathIgnored(Path.GetRelativePath(rootDirectory, file)));

                    // If the directory itself or its contents are not ignored, include it
                    if (hasVisibleFiles || !ignoredRules.IsPathIgnored(relativePath))
                    {
                        directoryListBuilder.AppendLine(relativePath);
                    }
                }
            }

            await _files.WriteTextToFile(filename, directoryListBuilder.ToString());

            return filename;
        }

    }
}