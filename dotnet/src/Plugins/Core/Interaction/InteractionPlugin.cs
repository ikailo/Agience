using Agience.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Interaction
{
    public class InteractionPlugin
    {
        private readonly IInteractionService _interaction;
        private readonly IServiceProvider _serviceProvider;

        public InteractionPlugin (IInteractionService interaction, IServiceProvider serviceProvider)
        {
            _interaction = interaction;
            _serviceProvider = serviceProvider;           
        }

        [KernelFunction, Description("Show a message")]
        public async Task Send(
            [Description("The message to show")] string message
            )
        {
            await _interaction.ReceiveFromAgent(_serviceProvider.GetRequiredService<Agent>(), message);
        }

        [KernelFunction, Description("Receive a message")]
        [return: Description("The received message")]
        public async Task<string> Receive()
        {
            return await _interaction.SendToAgent(_serviceProvider.GetRequiredService<Agent>()) ?? string.Empty;
        }

        [KernelFunction, Description("Show a message and collect a response")]
        [return: Description("The response message")]
        public async Task<string> Interact(
            [Description("The message to show")] string message)
        {
            await Send(message);
            return await Receive();
        }
    }
}