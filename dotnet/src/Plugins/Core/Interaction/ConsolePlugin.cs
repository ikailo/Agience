using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Interaction
{
    public sealed class ConsolePlugin
    {
        private readonly IConsoleService _console;

        public ConsolePlugin(IConsoleService console)
        {

            _console = console;
        }

        [KernelFunction, Description("Show a message to the person via the console.")]
        public async Task ShowMessageToPerson(
            [Description("The message to show to the person.")] string message
            )
        {
            await _console.WriteLineAsync(message);
        }

        [KernelFunction, Description("Get input from the person via the console.")]
        [return: Description("The person's input.")]
        public async Task<string> GetInputFromPerson()
        {
            return await _console.ReadLineAsync() ?? string.Empty;
        }

        [KernelFunction, Description("Interact with the person via the console. Send a message and receive a response.")]
        [return: Description("The person's response.")]
        public async Task<string> InteractWithPerson(
            [Description("The message to show to the person.")] string message)
        {
            // TODO: Here we can invoke directly, or invoke through the kernel. Going directly is faster, but going through the kernel could have benefits.
            await ShowMessageToPerson(message);
            return await GetInputFromPerson();
        }
    }
}