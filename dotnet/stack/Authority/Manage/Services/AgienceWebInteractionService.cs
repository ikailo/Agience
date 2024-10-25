using Agience.Plugins.Primary.Interaction;
using Agience.SDK;
using Agience.SDK.Logging;
using Agience.SDK.Models.Messages;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;

namespace Agience.Authority.Manage.Web.Services
{
    public class AgienceWebInteractionService : IInteractionService, IAgienceEventLogHandler
    {

        public event Func<string, Task>? AgencyConnected;
        public event Func<string, Task>? AgencyDisconnected;
        public event Func<AgienceEventLogArgs, Task>? AgencyLogEntryReceived;
        public event Func<AgienceChatMessageArgs, Task>? AgencyChatMessageReceived;

        public event Func<string, Task>? AgentConnected;
        public event Func<string, Task>? AgentDisconnected;
        public event Func<AgienceEventLogArgs, Task>? AgentLogEntryReceived;
        public event Func<AgienceChatMessageArgs, Task>? AgentChatMessageReceived;

        private readonly SDK.Host _host;
        private readonly ILogger<AgienceWebInteractionService> _logger;

        // In-memory storage for logs
        private readonly ConcurrentDictionary<string, List<string>> _agentLogs = new();
        private readonly ConcurrentDictionary<string, List<string>> _agencyLogs = new();

        public AgienceWebInteractionService(SDK.Host host, ILogger<AgienceWebInteractionService> logger)
        {
            _host = host;
            _logger = logger;

            // Agency Connect/Disconnect events
            _host.AgencyConnected += OnAgencyConnected;
            _host.AgencyDisconnected += OnAgencyDisconnected;
            //_host.Agencies.Values.ToList().ForEach(async agency => await OnAgencyConnected(agency));

            // Agent Connect/Disconnect events
            _host.AgentConnected += OnAgentConnected;
            _host.AgentDisconnected += OnAgentDisconnected;
            //_host.Agents.Values.ToList().ForEach(async agent => await OnAgentConnected(agent));
        }

        // Event handlers for host events
        private async Task OnAgencyConnected(Agency agency)
        {
            agency.ChatMessageReceived += OnAgencyChatMessageReceived;

            if (AgencyConnected != null)
            {
                await AgencyConnected.Invoke(agency.Id);
            }
        }

        private async Task OnAgentConnected(Agent agent)
        {
            agent.ChatMessageReceived += OnAgentChatMessageReceived;

            if (AgentConnected != null)
            {
                await AgentConnected.Invoke(agent.Id);
            }
        }

        private async Task OnAgencyDisconnected(string agencyId)
        {
            if (_host.Agencies.TryGetValue(agencyId, out var agency))
            {
                agency.ChatMessageReceived -= OnAgencyChatMessageReceived;
            }
            if (AgencyDisconnected != null)
            {
                await AgencyDisconnected.Invoke(agencyId);
            }
        }

        private async Task OnAgentDisconnected(string agentId)
        {
            if (_host.Agents.TryGetValue(agentId, out var agent))
            {
                agent.ChatMessageReceived -= OnAgentChatMessageReceived;
            }

            if (AgentDisconnected != null)
            {
                await AgentDisconnected.Invoke(agentId);
            }
        }


        // Handle incoming chat messages from agencies
        private async Task OnAgencyChatMessageReceived(AgienceChatMessageArgs message)
        {
            if (AgencyChatMessageReceived != null)
            {
                await AgencyChatMessageReceived.Invoke(message);
            }
        }

        // Handle incoming chat messages from agents
        private async Task OnAgentChatMessageReceived(AgienceChatMessageArgs message)
        {
            if (AgentChatMessageReceived != null)
            {
                await AgentChatMessageReceived.Invoke(message);
            }
        }

        // Handle log entries from agencies
        private void OnAgencyLogEntryReceived(AgienceEventLogArgs args)
        {
            if (args.AgencyId == null)
            {
                _logger.LogWarning("Log entry received without AgencyId: {LogMessage}", args.Formatter(args.State, args.Exception));

                return;
            }

            var logs = _agencyLogs.GetOrAdd(args.AgencyId, _ => new List<string>());

            lock (logs)
            {
                logs.Add(args.Formatter(args.State, args.Exception));
            }

            AgencyLogEntryReceived?.Invoke(args);
        }

        // Handle log entries from agents
        private void OnAgentLogEntryReceived(AgienceEventLogArgs args)
        {
            if (args.AgentId == null)
            {
                _logger.LogWarning("Log entry received without AgentId: {LogMessage}", args.Formatter(args.State, args.Exception));

                return;
            }

            var logs = _agentLogs.GetOrAdd(args.AgentId, _ => new List<string>());

            lock (logs)
            {
                logs.Add(args.Formatter(args.State, args.Exception));
            }

            AgentLogEntryReceived?.Invoke(args);
        }

        // Implement IAgienceEventLogHandler method
        public void OnLogEntryReceived(object? sender, AgienceEventLogArgs args)
        {
            if (args.AgentId != null)
            {
                OnAgentLogEntryReceived(args);
            }
            else if (args.AgencyId != null)
            {
                OnAgencyLogEntryReceived(args);
            }
            else
            {
                _logger.LogWarning($"Log entry received without valid AgentId/AgencyId. '{args.AgencyId}/{args.AgentId}'");
            }
        }

        public async Task SendAgencyChatMessageAsync(string agencyId, string message)
        {
            if (_host.Agencies.TryGetValue(agencyId, out var agency))
            {
                await agency.InformAsync(message);

            }
            _logger.LogInformation("Agency not found: {AgencyId}", agencyId);
        }

        public async Task SendAgentChatMessageAsync(string agentId, string message)
        {
            if (_host.Agents.TryGetValue(agentId, out var agent))
            {
                await agent.PromptAsync(message);
            }
            else
            {
                _logger.LogInformation("Agent not found: {AgentId}", agentId);
            }
        }


        // Chat and history methods wired directly to Agent and Agency methods
        public async Task<IEnumerable<ChatMessageContent>> GetAgencyChatHistoryAsync(string agencyId)
        {
            _logger.LogInformation("Getting chat history for agency: {AgencyId}", agencyId);
            if (_host.Agencies.TryGetValue(agencyId, out var agency))
            {
                return agency.ChatHistory;
            }
            return Enumerable.Empty<ChatMessageContent>();
        }

        public async Task<IEnumerable<ChatMessageContent>> GetAgentChatHistoryAsync(string agentId)
        {
            _logger.LogInformation("Getting chat history for agent: {AgentId}", agentId);
            if (_host.Agents.TryGetValue(agentId, out var agent))
            {
                return agent.ChatHistory;
            }
            return Enumerable.Empty<ChatMessageContent>();
        }

        // Log retrieval methods
        public Task<IEnumerable<string>> GetAgencyLogsAsync(string agencyId)
        {
            return Task.FromResult(_agencyLogs.TryGetValue(agencyId, out var logs) ? logs : Enumerable.Empty<string>());
        }

        public Task<IEnumerable<string>> GetAgentLogsAsync(string agentId)
        {

            return Task.FromResult(_agentLogs.TryGetValue(agentId, out var logs) ? logs : Enumerable.Empty<string>());
        }

        public Task<IEnumerable<Agency>> GetConnectedAgenciesAsync()
        {
            return Task.FromResult(_host.Agencies.Values.Where(a => a.IsConnected));
        }

        public Task<IEnumerable<Agent>> GetConnectedAgentsAsync()
        {
            return Task.FromResult(_host.Agents.Values.Where(a => a.IsConnected));
        }

        public Task<Agency?> GetConnectedAgencyAsync(string agencyId)
        {
            return Task.FromResult(_host.Agencies.Values.Where(a => a.IsConnected && a.Id == agencyId).FirstOrDefault());
        }

        public Task<Agent?> GetConnectedAgentAsync(string agentId)
        {
            return Task.FromResult(_host.Agents.Values.Where(a => a.IsConnected && a.Id == agentId).FirstOrDefault());
        }

        public Task<bool> IsAgencyConnected(string agencyId)
        {
            return Task.FromResult(_host.Agencies.TryGetValue(agencyId, out var agency) ? agency.IsConnected : false);
        }

        public Task<bool> IsAgentConnected(string agentId)
        {
            return Task.FromResult(_host.Agents.TryGetValue(agentId, out var agent) ? agent.IsConnected : false);
        }

        public bool? IsExistedAgentHandler()
        {
            //agent subscribers count 
            int? agentSubscriberCount = AgentLogEntryReceived?.GetInvocationList().Length;
            return agentSubscriberCount != null && agentSubscriberCount == 1 ? true : false;
        }

        public bool? IsExistedAgencyHandler()
        {
            //check agency subscribers count
            var agencySubscribersCount = AgentLogEntryReceived?.GetInvocationList().Length;
            return agencySubscribersCount != null && agencySubscribersCount == 1 ? true : false;
        }
    }
}