using Agience.SDK;
using Agience.SDK.Logging;
using Agience.SDK.Models.Messages;
using Microsoft.SemanticKernel;

namespace Agience.Plugins.Primary.Interaction
{
    public interface IInteractionService
    {
        // Connected Status Events
        event Func<string, Task>? AgencyConnected;
        event Func<string, Task>? AgentConnected;

        // Disconnected Status Events
        event Func<string, Task>? AgencyDisconnected;
        event Func<string, Task>? AgentDisconnected;

        // Log Events
        event Func<AgienceEventLogArgs, Task>? AgencyLogEntryReceived;
        event Func<AgienceEventLogArgs, Task>? AgentLogEntryReceived;

        // Incoming Chat Message Events
        event Func<AgienceChatMessageArgs, Task>? AgencyChatMessageReceived;
        event Func<AgienceChatMessageArgs, Task>? AgentChatMessageReceived;

        // Chat History Methods
        Task<IEnumerable<ChatMessageContent>> GetAgencyChatHistoryAsync(string agencyId);
        Task<IEnumerable<ChatMessageContent>> GetAgentChatHistoryAsync(string agentId);

        // Outgoing Chat Message Methods
        Task SendAgencyChatMessageAsync(string agencyId, string message);
        Task SendAgentChatMessageAsync(string agentId, string message);

        // Log Retrieval Methods
        Task<IEnumerable<string>> GetAgencyLogsAsync(string agencyId);
        Task<IEnumerable<string>> GetAgentLogsAsync(string agentId);

        // All Entities Retrieval Methods
        Task<IEnumerable<Agency>> GetConnectedAgenciesAsync();
        Task<IEnumerable<Agent>> GetConnectedAgentsAsync();

        // Single Entity Retrieval Methods
        Task<Agency?> GetConnectedAgencyAsync(string agencyId);
        Task<Agent?> GetConnectedAgentAsync(string agentId);

        // Connection Status Methods
        Task<bool> IsAgencyConnected(string agencyId);
        Task<bool> IsAgentConnected(string agentId);

        bool? IsExistedAgentHandler();
        bool? IsExistedAgencyHandler();
    }
}