//using Agience.Core;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.System
{
    public class Files
    {
        private readonly string _workingDirectory;

        public Files(string workspacePath)
        {
            _workingDirectory = workspacePath;
        }

        [KernelFunction, Description("Writes text content to a file.")]
        [return: Description("The filename that was written relative to the working directory.")]
        public async Task<string> WriteTextToFile(
            [Description("The filename to be written relative to the working directory.")] string filename, 
            [Description("The content to be written.")] string content
            )
        {
            string filePath = Path.Combine(_workingDirectory, filename);
            File.WriteAllText(filePath, content);
            return filename;
        }

    }
}
