using AutoMapper;
using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Agience.Core.Mappings;
using Agience.Core.Models.Messages;
using Microsoft.Extensions.DependencyInjection;
using Agience.Core.Interfaces;
using CoreModel=Agience.Core.Models.Entities;
using Microsoft.IdentityModel.Tokens;
using Agience.Core.Extensions;

namespace Agience.Core
{
    public class Authority
    {
        private const string BROKER_URI_KEY = "broker_uri";
        private const string OPENID_CONFIG_PATH = "/.well-known/openid-configuration";
               
        public string Id => _authorityUri.Host;
        public string? TokenEndpoint { get; private set; }
        public Uri? BrokerUri { get; private set; }        
        public bool IsConnected { get; private set; }
        public string Timestamp => _broker.Timestamp;

        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly Uri _authorityUri;
        private readonly Uri? _authorityUriInternal;
        private readonly Broker _broker;
        private readonly ILogger<Authority> _logger;
        private readonly TopicGenerator _topicGenerator;

        public Authority() { }

        public Authority(string authorityUri, Broker broker, IServiceScopeFactory serviceScopeFactory, ILogger<Authority> logger, Uri? authorityUriInternal = null, Uri? brokerUriInternal = null)
        {
            _authorityUri = !string.IsNullOrEmpty(authorityUri) ? new Uri(authorityUri) : throw new ArgumentNullException(nameof(authorityUri));
            _authorityUriInternal = authorityUriInternal;
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));            
            //_mapper = AutoMapperConfig.GetMapper();
            BrokerUri = brokerUriInternal;

            _topicGenerator = new TopicGenerator(Id, Id);
        }

        private IAuthorityRecordsRepository GetAuthorityRecordsRepository() => 
            _serviceScopeFactory.CreateScope().ServiceProvider.GetRequiredService<IAuthorityRecordsRepository>();

        internal async Task InitializeWithBackoff(double maxDelaySeconds = 16)
        {
            if (BrokerUri != null && !string.IsNullOrEmpty(TokenEndpoint))
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
                    
                    BrokerUri ??= new Uri(configuration?.AdditionalData[BROKER_URI_KEY].ToString()); // Don't reset it if it's already set
                    //FilesUri ??= configuration?.AdditionalData[FILES_URI_KEY].ToString(); // Don't reset it if it's already set

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
                if (BrokerUri == null)
                {
                    await InitializeWithBackoff();
                }

                var brokerUri = BrokerUri ?? throw new ArgumentNullException(nameof(BrokerUri));

                await _broker.Connect(accessToken, brokerUri);

                if (_broker.IsConnected)
                {
                    await _broker.Subscribe(_topicGenerator.SubscribeAsAuthority(), async message => await _broker_ReceiveMessage(message));
                    IsConnected = true;
                }                
            }
        }

        public async Task Disconnect()
        {
            if (IsConnected)
            {
                await _broker.Unsubscribe(_topicGenerator.SubscribeAsAuthority());
                await _broker.Disconnect();
                IsConnected = false;
            }
        }

        private async Task _broker_ReceiveMessage(BrokerMessage message)
        {
            _logger.LogInformation($"MessageReceived: sender:{message.SenderId}, destination:{message.Destination}");

            if (message.SenderId == null || message.Data == null) { return; }

            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "host_connect" &&
                message.Data?["host"] != null)
            {
                //_logger.LogDebug($"received host_connect: {message.Data?["host"]}");

                var host = JsonSerializer.Deserialize<CoreModel.Host>(message.Data?["host"]!);


                if (host?.Id == message.SenderId)
                {
                    await OnHostConnected(host);
                }
            }

            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "credential_request" &&
                message.Data?["credential_name"] != null &&
                message.Data?["jwk"] != null &&
                message.Data?["agent_id"] == message.SenderId
                )
            {
                var credentialName = message.Data?["credential_name"];
                var agentId = message.Data?["agent_id"];
                var jwk = JsonSerializer.Deserialize<JsonWebKey>(message.Data?["jwk"]);

                await HandleCredentialRequestAsync(agentId, credentialName, jwk);
            }
        }

        private async Task HandleCredentialRequestAsync(string agentId, string credentialName, JsonWebKey jwk)
        {
            var authorityRecordsRepository = GetAuthorityRecordsRepository();

            var credential = await authorityRecordsRepository.GetCredentialForAgentByName(agentId, credentialName);

            if (credential == null)
            {
                // Log and exit if the credential is not found or unavailable
                Console.WriteLine($"Credential '{credentialName}' not found for Agent '{agentId}'.");
                return;
            }

            // Encrypt the credential
            var encryptedCredential = jwk.EncryptWithJwk(credential);

            // Send the response back to the Agent
            await _broker.PublishAsync(new BrokerMessage
            {
                Type = BrokerMessageType.EVENT,
                Topic = _topicGenerator.PublishToAgent(agentId),
                Data = new Data
                {
                    { "type", "credential_response" },
                    { "credential_name", credentialName },
                    { "encrypted_credential", encryptedCredential }
                }
            });

            Console.WriteLine($"Credential response sent for '{credentialName}' to Agent '{agentId}'.");
        }


        private async Task OnHostConnected(CoreModel.Host modelHost)
        {

            var authorityRecordsRepository = GetAuthorityRecordsRepository();

            _logger.LogInformation($"Received host_connect from: {modelHost.Id}");

            var host = await authorityRecordsRepository.GetHostById(modelHost.Id);

            _logger.LogInformation($"Found Host {host.Name}");
            _logger.LogDebug($"Host: {JsonSerializer.Serialize(host)}");

            var plugins = await authorityRecordsRepository.SyncPluginsForHostById(modelHost.Id, modelHost.Plugins);

            _logger.LogInformation($"Found {plugins.Count()} Plugins");
            _logger.LogDebug($"Plugins: {JsonSerializer.Serialize(plugins)}");

            var agents = await authorityRecordsRepository.GetAgentsForHostById(modelHost.Id);

            _logger.LogInformation($"Found {agents.Count()} Agents");
            _logger.LogDebug($"Agents: {JsonSerializer.Serialize(agents)}");

            SendHostWelcomeEvent(host, plugins, agents);
        }


        private void SendHostWelcomeEvent(Models.Entities.Host host, IEnumerable<Models.Entities.Plugin> plugins, IEnumerable<Models.Entities.Agent> agents)
        {
            if (!IsConnected) { throw new InvalidOperationException("Not Connected"); }

            _logger.LogInformation($"Publishing Host Welcome Event: {host.Name}");

           // _logger.LogInformation($"Topic: {HostTopic(Id, host.Id)}");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _topicGenerator.PublishToHost(host.Id),
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

            var authorityRecordsRepository = GetAuthorityRecordsRepository();

            var hostId = await authorityRecordsRepository.GetHostIdForAgentById(agent.Id);

            _logger.LogInformation($"Sending Agent Connect Event: {agent.Name}");
            _logger.LogDebug($"Agent: {JsonSerializer.Serialize(agent)}");

            _broker.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _topicGenerator.PublishToHost(hostId),
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

            var authorityRecordsRepository = GetAuthorityRecordsRepository();

            var hostId = await authorityRecordsRepository.GetHostIdForAgentById(agent.Id);

            _broker!.Publish(new BrokerMessage()
            {
                Type = BrokerMessageType.EVENT,
                Topic = _topicGenerator.PublishToHost(hostId),
                Data = new Data
                {
                    { "type", "agent_disconnect" },
                    { "timestamp", _broker.Timestamp},
                    { "agent_id", agent.Id }
                }
            });
        }
    }
}
