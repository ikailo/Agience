using AutoMapper;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Agience.SDK.Mappings;
using Agience.SDK.Models.Entities;
using Agience.SDK.Models.Messages;

namespace Agience.SDK
{
    public class Authority
    {
        private const string BROKER_URI_KEY = "broker_uri";
        private const string OPENID_CONFIG_PATH = "/.well-known/openid-configuration";

        private readonly IAuthorityDataAdapter? _authorityDataAdapter;

        public string Id => _authorityUri.Host;
        public string? BrokerUri { get; private set; }
        public string? TokenEndpoint { get; private set; }
        public bool IsConnected { get; private set; }
        public string Timestamp => _broker.Timestamp;

        private readonly Uri _authorityUri; // Expect without trailing slash
        private readonly Uri? _authorityUriInternal; // For internal connections
        private readonly Broker _broker;
        private readonly ILogger<Authority> _logger;
        private readonly IMapper _mapper;

        public Authority() { }

        public Authority(string authorityUri, Broker broker, IAuthorityDataAdapter? authorityDataAdapter, ILogger<Authority> logger, string? authorityUriInternal = null, string? brokerUriInternal = null)
        {
            _authorityUri = !string.IsNullOrEmpty(authorityUri) ? new Uri(authorityUri) : throw new ArgumentNullException(nameof(authorityUri));
            _authorityUriInternal = authorityUriInternal == null ? null : new Uri(authorityUriInternal!);
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _authorityDataAdapter = authorityDataAdapter;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            
            _mapper = AutoMapperConfig.GetMapper();
            BrokerUri = brokerUriInternal;
        }

        internal async Task InitializeWithBackoff(double maxDelaySeconds = 16)
        {
            if (!string.IsNullOrEmpty(BrokerUri) && !string.IsNullOrEmpty(TokenEndpoint))
            {
                _logger.LogInformation("Authority already initialized.");
                return;
            }

            var delay = TimeSpan.FromSeconds(1);

            while (true)
            {
                try
                {
                    var authorityUri = _authorityUriInternal ?? _authorityUri;

                    _logger.LogInformation($"Initializing Authority: {authorityUri.OriginalString}");

                    var configurationManager = new ConfigurationManager<OpenIdConnectConfiguration>(
                        $"{authorityUri.OriginalString}{OPENID_CONFIG_PATH}",
                        new OpenIdConnectConfigurationRetriever()
                    );

                    var configuration = await configurationManager.GetConfigurationAsync();
                    
                    BrokerUri ??= configuration?.AdditionalData[BROKER_URI_KEY].ToString(); // Don't reset it if it's already set

                    // TODO: Better way needed to handle overrides/internal endpoints

                    if (_authorityUriInternal == null)
                    {
                        TokenEndpoint = configuration?.TokenEndpoint;
                    }
                    else if (configuration?.TokenEndpoint != null)
                    {
                        // Replace the host and port with the override
                        TokenEndpoint = new UriBuilder(configuration?.TokenEndpoint!)
                        {
                            Host = _authorityUriInternal.Host,
                            Port = _authorityUriInternal.Port

                        }.Uri.ToString();
                    }
                    else
                        {
                        throw new Exception("TokenEndpoint not found in OpenIdConnectConfiguration");
                    }

                    _logger.LogInformation($"Authority initialized.");

                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex.ToString());
                    _logger.LogInformation($"Unable to initialize Authority. Retrying in {delay.TotalSeconds} seconds.");

                    await Task.Delay(delay);

                    delay = TimeSpan.FromSeconds(Math.Min(delay.TotalSeconds * 2, maxDelaySeconds));
                }
            }
        }

        public async Task Connect(string accessToken)
        {
            if (!IsConnected)
            {
                if (string.IsNullOrEmpty(BrokerUri))
                {
                    await InitializeWithBackoff();
                }

                var brokerUri = BrokerUri ?? throw new ArgumentNullException("BrokerUri");

                await _broker.Connect(accessToken, brokerUri);

                if (_broker.IsConnected)
                {
                    await _broker.Subscribe(AuthorityTopic("+"), async message => await _broker_ReceiveMessage(message));
                    IsConnected = true;
                }                
            }
        }

        public async Task Disconnect()
        {
            if (IsConnected)
            {
                await _broker.Unsubscribe(AuthorityTopic("+"));
                await _broker.Disconnect();
                IsConnected = false;
            }
        }

        private async Task _broker_ReceiveMessage(BrokerMessage message)
        {
            if (message.SenderId == null || message.Data == null) { return; }

            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "host_connect" &&
                message.Data?["host"] != null)
            {
                var host = JsonSerializer.Deserialize<Models.Entities.Host>(message.Data?["host"]!);

                if (host?.Id == message.SenderId)
                {
                    await OnHostConnected(host.Id);
                }
            }
        }

        private async Task OnHostConnected(string hostId)
        {
            if (_authorityDataAdapter == null) { throw new ArgumentNullException(nameof(_authorityDataAdapter)); }

            _logger.LogInformation($"Received host_connect from: {hostId}");

            var host = await _authorityDataAdapter.GetHostByIdNoTrackingAsync(hostId);

            _logger.LogInformation($"Found Host {host.Name}");
            _logger.LogDebug($"Host: {JsonSerializer.Serialize(host)}");

            var plugins = await _authorityDataAdapter.GetPluginsForHostIdNoTrackingAsync(hostId);

            _logger.LogInformation($"Found {plugins.Count()} Plugins");
            _logger.LogDebug($"Plugins: {JsonSerializer.Serialize(plugins)}");

            var agents = await _authorityDataAdapter.GetAgentsForHostIdNoTrackingAsync(hostId);
                        
            _logger.LogInformation($"Found {agents.Count()} Agents");
            _logger.LogDebug($"Agents: {JsonSerializer.Serialize(agents)}");

            SendHostWelcomeEvent(host, plugins, agents);            
        }

        private void SendHostWelcomeEvent(Models.Entities.Host host, IEnumerable<Models.Entities.Plugin> plugins, IEnumerable<Models.Entities.Agent> agents)
        {
            if (!IsConnected) { throw new InvalidOperationException("Not Connected"); }

            _logger.LogInformation($"Publishing Host Welcome Event: {host.Name}");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = HostTopic(Id, host.Id),
                Data = new Data
                {
                    { "type", "host_welcome" },
                    { "timestamp", _broker.Timestamp},
                    { "host", JsonSerializer.Serialize(host) },
                    { "plugins", JsonSerializer.Serialize(plugins) },
                    { "agents", JsonSerializer.Serialize(agents) }
                }
            });
        }
        
        private async Task SendAgentConnectEvent(Models.Entities.Agent agent)
        {   
            if (!IsConnected) { throw new InvalidOperationException("Not Connected"); }

            if (_authorityDataAdapter == null) { throw new InvalidOperationException("AuthorityDataAdapter is missing"); }

            var hostId = await _authorityDataAdapter.GetHostIdForAgentIdNoTrackingAsync(agent.Id);

            _logger.LogInformation($"Sending Agent Connect Event: {agent.Name}");
            _logger.LogDebug($"Agent: {JsonSerializer.Serialize(agent)}");
            _logger.LogDebug($"Agency: {JsonSerializer.Serialize(agent.Agency)}");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = HostTopic(Id, hostId),
                Data = new Data
                {
                    { "type", "agent_connect" },
                    { "timestamp", _broker.Timestamp},
                    { "agent", JsonSerializer.Serialize(agent) }
                }
            });
        }

        private async Task SendAgentDisconnectEvent(Models.Entities.Agent agent)
        {
            if (!IsConnected) { throw new InvalidOperationException("Not Connected"); }

            if (_authorityDataAdapter == null) { throw new InvalidOperationException("AuthorityDataAdapter is missing"); }

            var hostId = await _authorityDataAdapter.GetHostIdForAgentIdNoTrackingAsync(agent.Id);

            _broker!.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = HostTopic(Id, hostId),
                Data = new Data
                {
                    { "type", "agent_disconnect" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", agent.Id }
                }
            });
        }

        internal string Topic(string senderId, string? hostId, string? agencyId, string? agentId)
        {
            var result = $"{(senderId != Id ? senderId : "-")}/{Id}/{hostId ?? "-"}/{agencyId ?? "-"}/{agentId ?? "-"}";
            return result;
        }

        internal string AuthorityTopic(string senderId)
        {
            return Topic(senderId, null, null, null);
        }

        internal string HostTopic(string senderId, string? hostId)
        {
            return Topic(senderId, hostId, null, null);
        }

        internal string AgencyTopic(string senderId, string agencyId)
        {
            return Topic(senderId, null, agencyId, null);
        }

        internal string AgentTopic(string senderId, string agentId)
        {
            return Topic(senderId, null, null, agentId);
        }

        public async Task AgentCreated(Models.Entities.Agent agent)
        {
            if (agent.IsEnabled)
            {
                await SendAgentConnectEvent(agent);
            }
        }

        public async Task AgentUpdated(Models.Entities.Agent agent)
        {
            await SendAgentDisconnectEvent(agent);

            if (agent.IsEnabled)
            {
                await SendAgentConnectEvent(agent);
            }
        }

        public async Task AgentDeleted(Models.Entities.Agent agent)
        {
            await SendAgentDisconnectEvent(agent);
        }
    }
}
