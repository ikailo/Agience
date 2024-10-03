//using Agience.SDK;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Files
{
    internal class Delete 
    {
        
        //public override string[] InputKeys => ["file_path"];
        /*
        [KernelFunction, Description("Delete a file on the local filesystem.")]
        public Task<Data?> Process(Runner runner, Data? input = null)
        {
            var fileName = input?["file_name"] ?? throw new ArgumentNullException("file_name");

            if (File.Exists(fileName))
            {
                File.Delete(fileName);
            }

            return Task.FromResult<Data?>(null);
        }*/
    }
}
