using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System.Collections.Concurrent;
using Timer = System.Timers.Timer;
using AutoMapper;
using Agience.SDK.Mappings;
using Agience.SDK.Models.Messages;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Agience.SDK
{
    [AutoMap(typeof(Models.Entities.Agent), ReverseMap = true)]
    public class Agent : Models.Entities.Agent, IDisposable
    {
        private const int JOIN_WAIT = 5000;
        public bool IsConnected { get; private set; }
        public new Agency Agency => _agency;
        public Kernel Kernel => _kernel;
        public string Timestamp => _broker.Timestamp;

        //private readonly ChatHistory _chatHistory;
        private readonly ConcurrentDictionary<string, Runner> _informationCallbacks = new();
        private readonly Timer _representativeClaimTimer = new Timer(JOIN_WAIT);
        private readonly Authority _authority;
        private readonly Agency _agency;
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
            //_logger = new AgienceLogger(logger, null, id);

            _mapper = AutoMapperConfig.GetMapper();
            //_chatHistory = new();

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

            if (IsConnected)
            {
                SendRepresentativeAbandon();
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

        private void SendRepresentativeAbandon()
        {
            if (_agency.RepresentativeId != Id) { return; } // Only the current representative can abandon

            _logger.LogDebug("SendRepresentativeAbandon");
            
            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _authority.AgencyTopic(Id!, _agency.Id!),
                Data = new Data
                {
                    { "type", "representative_abandon" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", Id },
                }
            });
        }

        public async Task<string> PromptAsync(string message, CancellationToken cancellationToken = default)
        {
            var chatHistory = new ChatHistory();

            chatHistory.AddUserMessage(message);

            var chatCompletionService = _kernel.GetRequiredService<IChatCompletionService>();

            var chatMessageContent = await chatCompletionService.GetChatMessageContentAsync(chatHistory, _promptExecutionSettings, _kernel, cancellationToken);

            return chatMessageContent.Items.Last().ToString() ?? string.Empty;
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