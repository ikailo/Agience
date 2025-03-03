using Agience.Core.Mappings;
using Agience.Core.Models.Enums;
using Agience.Core.Models.Messages;
using AutoMapper;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Reflection;
using System.Reflection.Metadata;
using System.Text;
using System.Text.Json;

namespace Agience.Core
{
    [AutoMap(typeof(Models.Entities.Host), ReverseMap = true)]
    public class Host : Models.Entities.Host
    {
        public event Func<Agent, Task>? AgentConnected;
        public event Func<string, Task>? AgentDisconnected;
        public Func<string, string, Task>? AgentLogEntryReceived;

        public bool IsConnected { get; private set; }
        public new IReadOnlyDictionary<string, Agent> Agents => _agents;

        private readonly string _hostSecret;
        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly AgentFactory _agentFactory;
        //private readonly PluginRuntimeLoader _pluginRuntimeLoader;
        private readonly ILogger<Host> _logger;
        private readonly IMapper _mapper;

        private readonly Dictionary<string, Agent> _agents = new();
        private readonly TopicGenerator _topicGenerator;

        internal readonly Dictionary<string, object> PluginInstances = new();

        internal Host(
            string hostId,
            string hostSecret,
            Authority authority,
            Broker broker,
            AgentFactory agentFactory,
            //PluginRuntimeLoader pluginRuntimeLoader,
            ILogger<Host> logger)
        {
            Id = !string.IsNullOrEmpty(hostId) ? hostId : throw new ArgumentNullException(nameof(hostId));
            _hostSecret = !string.IsNullOrEmpty(hostSecret) ? hostSecret : throw new ArgumentNullException(nameof(hostSecret));
            _authority = authority ?? throw new ArgumentNullException(nameof(authority));
            _broker = broker ?? throw new ArgumentNullException(nameof(broker));
            _agentFactory = agentFactory ?? throw new ArgumentNullException(nameof(agentFactory));
            //_pluginRuntimeLoader = pluginRuntimeLoader;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mapper = AutoMapperConfig.GetMapper();

            _topicGenerator = new TopicGenerator(_authority.Id, Id);
        }

        public async Task RunAsync()
        {
            await StartAsync();

            while (IsConnected)
            {
                await Task.Delay(100);
            }
        }

        public async Task Stop()
        {
            _logger.LogInformation("Stopping Host");

            await Disconnect();
        }

        public async Task StartAsync()
        {
            _logger.LogInformation("Starting Host");

            while (!IsConnected)
            {
                try
                {
                    await Connect();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to Connect");
                    _logger.LogInformation("Retrying in 10 seconds");

                    await Task.Delay(10 * 1000); // TODO: With backoff
                }
            }
        }

        private async Task Connect()
        {
            _logger.LogInformation("Connecting Host");

            await _authority.InitializeWithBackoff();

            var brokerUri = _authority.BrokerUri ?? throw new ArgumentNullException("BrokerUri");

            var accessToken = await GetAccessToken() ?? throw new ArgumentNullException("accessToken");

            await _broker.Connect(accessToken, _authority.BrokerUri.Host!, _authority.BrokerUri.Port);

            if (_broker.IsConnected)
            {
                //await _broker.Subscribe(_authority.HostTopic("+", "0"), _broker_ReceiveMessage); // Hosts Broadcast

                await _broker.Subscribe(_topicGenerator.SubscribeAsHost(), _broker_ReceiveMessage); // This Host

                

                await _broker.PublishAsync(new BrokerMessage()
                {
                    Type = BrokerMessageType.EVENT,
                    Topic = _topicGenerator.PublishToAuthority(),
                    Data = new Data
                {
                    { "type", "host_connect" },
                    { "timestamp", _broker.Timestamp},
                    { "host", JsonSerializer.Serialize(_mapper.Map<Models.Entities.Host>(this)) }
                }
                });

                IsConnected = true;
            }
            else
            {
                throw new Exception("Broker Connection Failed");
            }

            _logger.LogInformation("Host Connected");
        }

        private async Task Disconnect()
        {
            if (IsConnected)
            {
                foreach (Agent agent in _agents.Values)
                {
                    await agent.Disconnect();
                }

                //await _broker.Unsubscribe(_authority.HostTopic("+", "0"));
                await _broker.Unsubscribe(_topicGenerator.SubscribeAsHost());

                await _broker.Disconnect();

                IsConnected = false;
            }
        }

        private async Task<string?> GetAccessToken()
        {
            var clientSecret = _hostSecret;
            var tokenEndpoint = _authority.TokenEndpoint ?? throw new ArgumentNullException("tokenEndpoint");

            // TODO: Use a shared HttpClient host
            using (var httpClient = new HttpClient())
            {
                var basicAuthHeader = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Id}:{clientSecret}"));
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", basicAuthHeader);

                var parameters = new Dictionary<string, string>
                {
                    { "grant_type", "client_credentials" },
                    { "scope", "connect" }
                };

                var httpResponse = await httpClient.PostAsync(tokenEndpoint, new FormUrlEncodedContent(parameters));

                if (httpResponse.IsSuccessStatusCode)
                {
                    return (await httpResponse.Content.ReadFromJsonAsync<TokenResponse>())?.access_token;
                }

                return null;
            }
        }

        public void AddPlugin<T>(T instance) where T : class
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var type = instance.GetType();
            var pluginName = type.FullName ?? type.Name;

            // Ensure the instance is unique (optional: overwrite existing instances)
            if (PluginInstances.ContainsKey(pluginName))
            {
                throw new InvalidOperationException($"A plugin with the name '{pluginName}' already exists.");
            }

            // Store the instance
            PluginInstances[pluginName] = instance;

            // Initialize a new plugin instance
            var plugin = new Models.Entities.Plugin
            {
                Name = type.Name,
                UniqueName = type.FullName,
                Description = string.Empty,
                PluginProvider = PluginProvider.SKPlugin,
                PluginSource = PluginSource.HostDefined,
                Type = type
            };

            // Retrieve all methods decorated with KernelFunctionAttribute
            var decoratedMethods = type
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), false).Any())
                .ToList();

            // Map methods to Function objects
            foreach (var method in decoratedMethods)
            {
                var function = new Models.Entities.Function
                {
                    Name = method.Name,
                    Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                    Inputs = method.GetParameters().Select(param => new Models.Entities.Parameter
                    {
                        Name = param.Name ?? string.Empty,
                        Description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                        Type = GetFriendlyTypeName(param.ParameterType)
                    }).ToList(),
                    Outputs = new List<Models.Entities.Parameter>
                    {
                        new Models.Entities.Parameter
                        {
                            Name = "result",
                            Description = method.ReturnParameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                            Type = GetFriendlyTypeName(method.ReturnType)
                        }
                    }
                };

                plugin.Functions.Add(function);
            }

            // Add the plugin to the list
            Plugins.Add(plugin);
        }

        private string GetFriendlyTypeName(Type type)
        {
            if (type.IsGenericType)
            {
                var typeName = type.Name.Split('`')[0];
                var genericArgs = string.Join(", ", type.GetGenericArguments().Select(GetFriendlyTypeName));
                return $"{typeName}<{genericArgs}>";
            }

            return type.Name;
        }
    

        public void AddPluginFromType<T>() where T : class
        {
            // Initialize a new plugin instance
            var plugin = new Models.Entities.Plugin
            {
                Name = typeof(T).Name,
                UniqueName = typeof(T).FullName,
                Description = string.Empty,
                PluginProvider = PluginProvider.SKPlugin,
                PluginSource = PluginSource.HostDefined
            };

            // Retrieve all methods decorated with KernelFunctionAttribute
            var decoratedMethods = typeof(T)
                .GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)
                .Where(m => m.GetCustomAttributes(typeof(KernelFunctionAttribute), false).Any())
                .ToList();

            // Map methods to Function objects
            foreach (var method in decoratedMethods)
            {
                var function = new Models.Entities.Function
                {
                    Name = method.Name,

                    Description = method.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,

                    Inputs = method.GetParameters().Select(param => new Models.Entities.Parameter
                    {
                        Name = param.Name ?? string.Empty,
                        Description = param.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                        // Ensure the type name for parameters is clear
                        Type = param.ParameterType.IsGenericType
                            ? $"{param.ParameterType.Name.Split('`')[0]}<{string.Join(", ", param.ParameterType.GetGenericArguments().Select(arg => arg.Name))}>"
                            : param.ParameterType.Name
                    }).ToList(),

                    Outputs = new List<Models.Entities.Parameter>
                        {
                            new Models.Entities.Parameter
                            {
                                Name = "result",
                                Description = method.ReturnParameter.GetCustomAttribute<DescriptionAttribute>()?.Description ?? string.Empty,
                                // Ensure the type name for the return type is clear
                                Type = method.ReturnType.IsGenericType
                                    ? $"{method.ReturnType.Name.Split('`')[0]}<{string.Join(", ", method.ReturnType.GetGenericArguments().Select(arg => arg.Name))}>"
                                    : method.ReturnType.Name
                            }
                        }
                };

                plugin.Functions.Add(function);
            }

            plugin.Type = typeof(T);

            Plugins.Add(plugin);

        }

        private async Task ReceiveHostWelcome(Models.Entities.Host host, IEnumerable<Models.Entities.Plugin> plugins, IEnumerable<Models.Entities.Agent> agents)
        {
            foreach (var plugin in plugins)
            {
                // Reconcile Plugin IDs
                var existingPlugin = Plugins.FirstOrDefault(p => p.UniqueName == plugin.UniqueName)
                                     ?? Plugins.FirstOrDefault(p => p.Name == plugin.Name);

                if (existingPlugin != null)
                {
                    // Update Plugin ID
                    existingPlugin.Id = plugin.Id;

                    // Reconcile Function IDs within the existing plugin
                    foreach (var function in plugin.Functions)
                    {
                        var existingFunction = existingPlugin.Functions.FirstOrDefault(f => f.Name == function.Name);
                        if (existingFunction != null)
                        {
                            // Update Function ID
                            existingFunction.Id = function.Id;
                        }
                        else
                        {
                            // Add new function
                            //existingPlugin.Functions.Add(function);
                        }
                    }

                    // Remove extra functions from the existing plugin that are not in the current plugin
                    //var functionNames = plugin.Functions.Select(f => f.Name).ToHashSet();
                    //existingPlugin.Functions.RemoveAll(f => !functionNames.Contains(f.Name));
                }
                else
                {
                    // Add the entire plugin, including its functions
                    Plugins.Add(plugin);
                }
            }


            foreach (var agent in agents)
            {
                await ReceiveAgentConnect(agent);
            }
        }

        private async Task ReceiveAgentConnect(Models.Entities.Agent modelAgent)
        {
            // Creates an Agent configured with the plugins and functions.
            // Agent instantiation is initiated from Authority. The Host does not have control.

            var agent = _agentFactory.CreateAgent(modelAgent);

            // Connect the Agent now
            _agents[agent.Id!] = agent;

            await agent.Connect();

            _logger.LogInformation($"{agent.Name} Connected");

            // TODO: Auto Start Function?


            if (AgentConnected != null)
            {
                await AgentConnected.Invoke(agent);
            }

            //var response = await agent.PromptAsync("Hello");

            // agent.AutoStart();

            //  *******************************
            // TODO: Add remote plugins/functions (MQTT, GRPC, HTTP) that we want the Agent Kernels to consider local.
            // TODO: Probably this should be done in the Functions themselves, so it can be dynamic and lazy initialized.
            // _host.ImportPluginFromGrpcFile("path-to.proto", "plugin-name");
            //  *******************************
        }

        private async Task ReceiveAgentDisconnect(string agentId)
        {
            var agent = _agents[agentId];

            await agent.Disconnect();

            _logger.LogInformation($"{agent.Name} Disconnected");

            if (AgentDisconnected != null)
            {
                await AgentDisconnected.Invoke(agentId);
            }

            _agentFactory.DisposeAgent(agentId);
        }


        private async Task _broker_ReceiveMessage(BrokerMessage message)
        {
            if (message.SenderId == null || message.Data == null) { return; }


            // Incoming Host Welcome Message
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "host_welcome" &&
                message.Data?["host"] != null)
            {
                var host = JsonSerializer.Deserialize<Models.Entities.Host>(message.Data?["host"]!);
                var plugins = JsonSerializer.Deserialize<IEnumerable<Models.Entities.Plugin>>(message.Data?["plugins"]!) ?? [];
                var agents = JsonSerializer.Deserialize<IEnumerable<Models.Entities.Agent>>(message.Data?["agents"]!) ?? [];

                if (host?.Id == null)
                {
                    _logger.LogError("Invalid Host");
                }
                else
                {
                    _logger.LogInformation($"Received Host Welcome Message for {host.Name}");

                    await ReceiveHostWelcome(host, plugins, agents);
                }
            }

            // Incoming Agent Connect Message
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "agent_connect" &&
                message.Data?["agent"] != null)
            {
                var agent = JsonSerializer.Deserialize<Models.Entities.Agent>(message.Data?["agent"]!);

                _logger.LogInformation($"ReceiveAgentConnect for {agent?.Id}");

                if (string.IsNullOrWhiteSpace(agent?.Id))
                {
                    _logger.LogError("Invalid Agent");
                    return;
                }
                await ReceiveAgentConnect(agent);

            }

            // Incoming Agent Disconnect Message
            if (message.Type == BrokerMessageType.EVENT &&
                message.Data?["type"] == "agent_disconnect" &&
                message.Data?["agent_id"] != null)
            {
                var agentId = message.Data?["agent_id"];

                await ReceiveAgentDisconnect(agentId);
            }
        }

        internal class TokenResponse
        {
            public string? access_token { get; set; }
            public string? token_type { get; set; }
            public int? expires_in { get; set; }
        }
    }
}