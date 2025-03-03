//using Agience.Core;
//using Agience.Core.Models;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Text
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
