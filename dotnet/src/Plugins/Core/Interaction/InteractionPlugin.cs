using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Agience.Plugins.Core.Interaction
{
    public class InteractionPlugin
    {
        /*
        private readonly IInteractionService _interactionService;
        private readonly IConsoleService _console;

        public InteractionPlugin(IInteractionService interactionService, IConsoleService console)
        {
            _interactionService = interactionService;
            _console = console;

            _interactionService.AgencyChatMessageReceived += OnAgencyChatMessageReceived;
        }

        [KernelFunction, Description("Prompt an agent and get a response.")]
        [return: Description("The agent's response.")]
        public async Task<string> PromptAgent(
            [Description("The ID of the agent to prompt.")] string agentId,
            [Description("The message to send to the agent.")] string message)
        {
            return await _interactionService.PromptAgentAsync(agentId, message);
        }

        [KernelFunction, Description("Inform an agency of a message.")]
        [return: Description("A boolean indicating success or failure.")]
        public async Task<bool> InformAgency(
            [Description("The ID of the agency to inform.")] string agencyId,
            [Description("The message to send to the agency.")] string message)
        {
            return await _interactionService.InformAgencyAsync(agencyId, message);
        }

        [KernelFunction, Description("Get the history of interactions with an agency.")]
        [return: Description("A list of history entries.")]
        public async Task<IEnumerable<ChatMessageContent>> GetAgencyHistory(
            [Description("The ID of the agency whose history to retrieve.")] string agencyId)
        {
            return await _interactionService.GetAgencyChatHistoryAsync(agencyId);
        }

        private async Task OnAgencyChatMessageReceived(string agencyId, IEnumerable<ChatMessageContent> message)
        {
            throw new NotImplementedException();
        }*/
    }
}
