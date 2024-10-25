using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Files
{
    internal class Append 
    {
        /*
        //public override string[] InputKeys => ["file_path", "content"];

        [KernelFunction, Description("Append text to file in the local filesystem.")]
        public async Task<Data?> Process(Runner runner, Data? input = null)
        {
            using (var writer = new StreamWriter(input?["file_path"] ?? throw new ArgumentNullException("file_path")))
            {
                await writer.WriteAsync(input?["content"]);
            }
            return null;
        }*/
    }
}
