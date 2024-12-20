using Agience.Plugins.Core.Interaction;
using Agience.Core;
using Agience.Core.Logging;
using Agience.Core.Models.Messages;
using Microsoft.SemanticKernel;
using System.Collections.Concurrent;
using Agience.Core.Interfaces;

namespace Agience.Authority.Manage.Services
{
    public class AgienceWebInteractionService : IInteractionService, IEventLogHandler
    {

        

        public event Func<string, Task>? AgentConnected;
        public event Func<string, Task>? AgentDisconnected;
        public event Func<EventLogArgs, Task>? AgentLogEntryReceived;
        public event Func<AgienceChatMessageArgs, Task>? AgentChatMessageReceived;

        private readonly Core.Host _host;
        private readonly ILogger<AgienceWebInteractionService> _logger;

        // In-memory storage for logs
        private readonly ConcurrentDictionary<string, List<string>> _agentLogs = new();
        

        public AgienceWebInteractionService(Core.Host host, ILogger<AgienceWebInteractionService> logger)
        {
            _host = host;
            _logger = logger;

        
            // Agent Connect/Disconnect events
            _host.AgentConnected += OnAgentConnected;
            _host.AgentDisconnected += OnAgentDisconnected;
            //_host.Agents.Values.ToList().ForEach(async agent => await OnAgentConnected(agent));
        }
               

        private async Task OnAgentConnected(Agent agent)
        {
            agent.ChatMessageReceived += OnAgentChatMessageReceived;

            if (AgentConnected != null)
            {
                await AgentConnected.Invoke(agent.Id);
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


        // Handle incoming chat messages from agents
        private async Task OnAgentChatMessageReceived(AgienceChatMessageArgs message)
        {
            if (AgentChatMessageReceived != null)
            {
                await AgentChatMessageReceived.Invoke(message);
            }
        }


        // Handle log entries from agents
        private void OnAgentLogEntryReceived(EventLogArgs args)
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
        public void OnLogEntryReceived(object? sender, EventLogArgs args)
        {
            if (args.AgentId != null)
            {
                OnAgentLogEntryReceived(args);
            }
            else
            {
                _logger.LogWarning($"Log entry received without valid AgentId. '{args.AgentId}'");
            }
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

        public async Task<IEnumerable<ChatMessageContent>> GetAgentChatHistoryAsync(string agentId)
        {
            _logger.LogInformation("Getting chat history for agent: {AgentId}", agentId);
            if (_host.Agents.TryGetValue(agentId, out var agent))
            {
                return agent.ChatHistory;
            }
            return Enumerable.Empty<ChatMessageContent>();
        }


        public Task<IEnumerable<string>> GetAgentLogsAsync(string agentId)
        {

            return Task.FromResult(_agentLogs.TryGetValue(agentId, out var logs) ? logs : Enumerable.Empty<string>());
        }

        public Task<IEnumerable<Agent>> GetConnectedAgentsAsync()
        {
            return Task.FromResult(_host.Agents.Values.Where(a => a.IsConnected));
        }

        public Task<Agent?> GetConnectedAgentAsync(string agentId)
        {
            return Task.FromResult(_host.Agents.Values.Where(a => a.IsConnected && a.Id == agentId).FirstOrDefault());
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
    }
}