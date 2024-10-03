using AutoMapper;
using Agience.SDK.Mappings;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Agience.SDK.Models.Messages;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel;

namespace Agience.SDK
{
    [AutoMap(typeof(Models.Entities.Agency), ReverseMap = true)]
    public class Agency : Models.Entities.Agency
    {
        // TODO: Agent Orchestration via Agency. One such method: ACE Layer processing
        // https://github.com/daveshap/ACE_Framework/blob/main/publications/Conceptual%20Framework%20for%20Autonomous%20Cognitive%20Entities%20(ACE).pdf
        // https://github.com/daveshap/ACE_Framework/blob/main/ACE_PRIME/HelloAF/src/ace/resources/core/hello_layers/prompts/templates/ace_context.md

        public event Func<IEnumerable<ChatMessageContent>, Task>? HistoryUpdated;

        public bool IsConnected { get; private set; }
        internal string? RepresentativeId { get; private set; }
        public string Timestamp => _broker.Timestamp;

        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly ILogger<Agency> _logger;
        private readonly IMapper _mapper;
        
        private readonly Dictionary<string, DateTime> _agentJoinTimestamps = new();
        private readonly Dictionary<string, Agent> _agents = new();
        private readonly List<string> _localAgentIds = new();        

        private readonly ChatHistory _history = new(); // TODO: History should be persistent.
        //private readonly History _history; // TODO: Use History instead of ChatHistory

        internal Agency(Authority authority, Broker broker, ILogger<Agency> logger)
        {
            _authority = authority;
            _broker = broker;
            _logger = logger;
            _mapper = AutoMapperConfig.GetMapper();
            //_history = new History(null, Id); // TODO: use idProvider to get a unique id
        }

        internal async Task Connect()
        {
            if (!IsConnected)
            {
                await _broker.Subscribe(_authority.AgencyTopic("+", Id!), _broker_ReceiveMessage);
                IsConnected = true;
            }
        }

        internal async Task Disconnect()
        {
            if (IsConnected)
            {
                await _broker.Unsubscribe(_authority.AgencyTopic("+", Id!));
                IsConnected = false;
            }
        }

        private Task _broker_ReceiveMessage(BrokerMessage message)
        {
            if (message.SenderId == null) { return Task.CompletedTask; }

            // Incoming Agent Join message
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "join" &&
                message.Data?["agent"] != null &&
                message.Data?["timestamp"] != null)
            {
                var timestamp = DateTime.TryParse(message.Data?["timestamp"], out DateTime result) ? (DateTime?)result : null;
                var agentId = message.Data?["agent_id"];

                if (agentId == message.SenderId && timestamp != null)
                {
                    ReceiveJoin(agentId, (DateTime)timestamp);
                }
            }

            // Incoming Representative Claim message
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "representative_claim" &&
                message.Data?["agent_id"] != null &&
                message.Data?["timestamp"] != null)
            {
                var timestamp = DateTime.TryParse(message.Data?["timestamp"], out DateTime result) ? (DateTime?)result : null;
                var agentId = message.Data?["agent_id"];

                if (agentId == message.SenderId && timestamp != null)
                {
                    ReceiveRepresentativeClaim(agentId, timestamp);
                }
            }
            return Task.CompletedTask;
        }

        private void ReceiveJoin(string agentId, DateTime timestamp)
        {
            _logger.LogInformation($"ReceiveJoin {_agents[agentId].Name}");

            _agentJoinTimestamps[agentId] = timestamp;            

            if (RepresentativeId != null && _localAgentIds.Contains(RepresentativeId))
            {
                //SendAgentWelcome(agentId);
            }
        }

        /*
        private void SendAgentWelcome(string agentId)
        {
            _logger.LogInformation($"SendAgentWelcome to {_agents[agentId].Name} with {_agents.Values.Count} Agents.");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgentTopic(Id!, agent.Id!),
                Data = new Data
                {
                    { "type", "welcome" },
                    { "timestamp", _broker.Timestamp },
                    { "agency", JsonSerializer.Serialize(_mapper.Map<Models.Entities.Agency>(this)) },
                    { "representative_id", RepresentativeId },
                    //{ "agents", JsonSerializer.Serialize(_agents.Values.Select(a => a.Item1).ToList()) },
                    //{ "agent_timestamps", JsonSerializer.Serialize(_agents.ToDictionary(a => a.Key, a => a.Value.Item2)) }
                }
            });
        }*/

        /*
        internal void ReceiveWelcome(Models.Entities.Agency agency,
                                     string representativeId,
                                     List<Models.Entities.Agent> agents,
                                     Dictionary<string, DateTime> agentTimestamps,                                            
                                     DateTime timestamp)
        {
            _logger?.LogInformation($"ReceiveWelcome from {agency.Name} {GetAgentName(representativeId)}");

            if (RepresentativeId != representativeId)
            {
                RepresentativeId = representativeId;
                _logger?.LogInformation($"Set Representative {GetAgentName(RepresentativeId)}");
            }

            foreach (var agent in agents)
            {
                _agents[agent.Id!] = (agent, agentTimestamps[agent.Id!]);
            }
        }
        */

        // TODO: Handle race conditions
        // Network Latency, Simultaneous Joins, etc.
        private void ReceiveRepresentativeClaim(string agentId, DateTime? timestamp)
        {
            _logger.LogInformation($"ReceiveRepresentativeClaim from {_agents[agentId].Name}");

            if (RepresentativeId != agentId)
            {
                RepresentativeId = agentId;
                _logger?.LogInformation($"Set Representative {GetAgentName(RepresentativeId)}");
            }

            /*
            if (_localAgentIds.Contains(RepresentativeId))
            {
                var repJoinTime = _agentJoinTimestamps[RepresentativeId];
                foreach (var agent in _agentJoinTimestamps.Where(a => a.Value >= repJoinTime))
                {
                    SendAgentWelcome(agent.Value);
                }
            }*/
        }

        internal string? GetAgentName(string agentId)
        {
            return _agents.TryGetValue(agentId, out Agent? agent) ? agent.Name : null;
        }

        internal void AddLocalAgent(Agent agent)
        {
            _agents[agent.Id] = agent;
            _localAgentIds.Add(agent.Id);
        }

        internal Agent? GetLocalAgent(string agentId)
        {
            return _localAgentIds.Contains(agentId) ? _agents[agentId] : null;
        }
                
        public async Task InformAsync(string message)
        {
            var chatMessage = new ChatMessageContent(AuthorRole.User, message);
            _history.Add(chatMessage);
            HistoryUpdated?.Invoke([chatMessage]);
        }

        public async Task<IEnumerable<ChatMessageContent>> GetHistory()
        {
            return _history;
        }
    }
}
