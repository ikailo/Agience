//using Agience.SDK;
//using Agience.SDK.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Primary.Text
{
    public class CharacterLength //: IAgiencePlugin
    {
        [KernelFunction, Description("Count the number of characters in the input including whitespace.")]
        public Task<int?> Process(string input)
        {
            return Task.FromResult(input?.ToString()?.Length);
        }
    }
}
