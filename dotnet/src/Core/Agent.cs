using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;
using AutoMapper;
using Agience.Core.Mappings;
using Agience.Core.Models.Messages;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Agience.Core
{
    [AutoMap(typeof(Models.Entities.Agent), ReverseMap = true)]
    public class Agent : Models.Entities.Agent, IDisposable
    {
        private const int JOIN_WAIT = 5000;
        public bool IsConnected { get; private set; }

        public event Func<AgienceChatMessageArgs, Task>? ChatMessageReceived;
        public ChatHistory ChatHistory => _chatHistory;        
        public new Agency Agency => _agency;
        public Kernel Kernel => _kernel;
        public string Timestamp => _broker.Timestamp;


        private readonly ChatHistory _chatHistory = new();
        private readonly ConcurrentDictionary<string, Runner> _informationCallbacks = new();
        private readonly Timer _representativeClaimTimer = new Timer(JOIN_WAIT);
        private readonly Authority _authority;
        private readonly Agency _agency;


        // TODO: Use Messenger instead of Broker. Abstrct MQTT Connection to a Messaging Service
        private readonly Broker _broker;
        private readonly ILogger _logger;
        private readonly Kernel _kernel;
        private readonly IMapper _mapper;
        private bool _disposed;

        private PromptExecutionSettings? _promptExecutionSettings;
        private string _persona;


        internal Agent(
            string id,
            string name,
            Authority authority,
            Broker broker,
            Agency agency,
            string persona,
            Kernel kernel,
            ILogger<Agent> logger
            )

        {
            Id = id;
            Name = name;

            _authority = authority;
            _broker = broker;
            _agency = agency;
            _persona = persona;
            _kernel = kernel;
            _logger = logger;
            _mapper = AutoMapperConfig.GetMapper();

            _promptExecutionSettings = new OpenAIPromptExecutionSettings
            {
                ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions
            };

            _representativeClaimTimer.AutoReset = false;
            _representativeClaimTimer.Elapsed += (s, e) => SendRepresentativeClaim();

            
        }

        internal async Task Connect()
        {
            if (!IsEnabled) { return; } // TODO: Log or handle this

            if (!IsConnected)
            {
                await _broker.Subscribe(_authority.AgentTopic("+", Id!), _broker_ReceiveMessage);

                SendJoin();
                _representativeClaimTimer.Start();

                IsConnected = true;
            }
        }

        private async Task _broker_ReceiveMessage(BrokerMessage message)
        {
            throw new NotImplementedException();
        }

        internal async Task Disconnect()
        {
            // TODO: Auto-disconnect via MQTT Will message
            // TODO: Abstract the MQTT Connection to a Messaging Service

            if (IsConnected)
            {
                SendRepresentativeResign();
                SendLeave();
                await _broker.Unsubscribe(_authority.AgentTopic("+", Id!));
                IsConnected = false;
            }
        }

        private void SendJoin()
        {
            _logger.LogDebug("SendJoin");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgencyTopic(Id!, _agency.Id!),
                Data = new Data
                {
                    { "type", "join" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id" , Id }
                }
            });
        }

        private void SendLeave()
        {
            _logger.LogDebug("SendLeave");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgencyTopic(Id!, _agency.Id!),
                Data = new Data
                {
                    { "type", "leave" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", Id }
                }
            });
        }

        private void SendRepresentativeClaim()
        {
            if (_agency.RepresentativeId != null) { return; } // Was set by another agent

            _logger.LogDebug("SendRepresentativeClaim");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgencyTopic(Id!, _agency.Id!),
                Data = new Data
                {
                    { "type", "representative_claim" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", Id },
                }
            });
        }

        private void SendRepresentativeResign()
        {
            if (_agency.RepresentativeId != Id) { return; } // Only the current representative can resign

            _logger.LogDebug("SendRepresentativeResign");
            
            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgencyTopic(Id!, _agency.Id!),
                Data = new Data
                {
                    { "type", "representative_resign" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", Id },
                }
            });
        }

        public async Task PromptAsync(string userMessage, CancellationToken cancellationToken = default)
        {
            _chatHistory.AddUserMessage(userMessage);

            var chatMessageContent = await _kernel.GetRequiredService<IChatCompletionService>()
                .GetChatMessageContentAsync(_chatHistory, _promptExecutionSettings, _kernel, cancellationToken);

            if (chatMessageContent?.Items.Last().ToString() is string assistantMessage)
            {
                _chatHistory.AddAssistantMessage(assistantMessage);

                ChatMessageReceived?.Invoke(new AgienceChatMessageArgs() { AgentId = Id, Message = chatMessageContent });
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                if (_logger is IDisposable disposableLogger)
                {
                    disposableLogger.Dispose();
                }
                _disposed = true;
            }
        }
    }
}