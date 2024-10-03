using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Files
{
    internal class Read
    {
        [KernelFunction, Description("Read a text file on the local filesystem.")]
        public async Task<string?> Process(string filename)
        {

            using StreamReader reader = new StreamReader(filename);
            return await reader.ReadToEndAsync();
        }
    }
}
