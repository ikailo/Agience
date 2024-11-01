using Agience.Core.Extensions;
using Agience.Core.Logging;
using Agience.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using NuGet.Protocol.Plugins;

namespace Agience.Core
{
    internal class AgentFactory : IDisposable
    {
        private readonly IServiceProvider _mainServiceProvider;
        private readonly ILogger<AgentFactory> _logger;
        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly string _hostOpenAiApiKey;
        private readonly Dictionary<string, Type> _hostPluginsCompiled = new();
        private readonly Dictionary<string, KernelPlugin> _hostPluginsCurated = new();
        private readonly Dictionary<string, Agency> _agencies = new();
        private readonly List<Agent> _agents = new();

        internal AgentFactory(
            IServiceProvider mainServiceProvider,
            Authority authority,
            Broker broker,
            ILogger<AgentFactory> logger,
            string? hostOpenAiApiKey = null
            )
        {
            _mainServiceProvider = mainServiceProvider.CreateScope().ServiceProvider;
            _authority = authority;
            _broker = broker;
            _logger = logger;
            _hostOpenAiApiKey = hostOpenAiApiKey ?? string.Empty;
        }

        internal void AddHostPluginFromType<T>(string pluginName) where T : class
        {
            _hostPluginsCompiled.Add(pluginName, typeof(T));
        }

        internal void AddHostPlugin(Models.Entities.Plugin plugin)
        {
            if (string.IsNullOrWhiteSpace(plugin.Name))
            {
                _logger.LogWarning("Plugin name is empty. Plugin will not be loaded.");
                return;
            }

            if (_hostPluginsCompiled.ContainsKey(plugin.Name) || _hostPluginsCurated.ContainsKey(plugin.Name))
            {
                _logger.LogWarning($"{plugin.Name} is already loaded. Plugin will not be loaded.");
                return;
            }

            if (plugin.Type == Models.Entities.PluginType.Compiled)
            {
                var pluginType = Type.GetType(plugin.Name);

                if (pluginType != null)
                {
                    _hostPluginsCompiled.Add(plugin.Name, pluginType);
                }
            }
            else if (plugin.Type == Models.Entities.PluginType.Curated)
            {
                _hostPluginsCurated.Add(plugin.Name, CreateKernelPluginCurated(plugin));
            }
        }

        internal Agent CreateAgent(Models.Entities.Agent modelAgent)
        {
            var agentPlugins = new KernelPluginCollection();

            // Create a new ServiceCollection for the Kernel
            var kernelServiceCollection = new ServiceCollection();

            kernelServiceCollection.AddSingleton<ILoggerFactory>(sp =>
            {
                var agienceEventLoggerFactory = new AgienceEventLoggerFactory(modelAgent.AgencyId, modelAgent.Id);

                var agienceEventLoggerProvider = _mainServiceProvider.GetRequiredService<AgienceEventLoggerProvider>();

                agienceEventLoggerFactory.AddProvider(agienceEventLoggerProvider);

                foreach (var provider in _mainServiceProvider.GetServices<ILoggerProvider>())
                {
                    if (provider != agienceEventLoggerProvider)
                    {
                        agienceEventLoggerFactory.AddProvider(provider);
                    }
                }

                return agienceEventLoggerFactory;
            });

            // Add or override services specific to this Kernel

            var credentialService = new AgienceCredentialService(modelAgent.Id, _authority, _broker);
            if (!string.IsNullOrWhiteSpace(_hostOpenAiApiKey))
            {
                credentialService.AddCredential("HostOpenAiApiKey", _hostOpenAiApiKey);
            }
            kernelServiceCollection.AddSingleton(credentialService);

            using var tempServiceProvider = kernelServiceCollection.BuildServiceProvider();

            foreach (var plugin in modelAgent.Plugins)
            {
                if (string.IsNullOrWhiteSpace(plugin.Name))
                {
                    _logger.LogWarning("Plugin name is empty.");
                    continue;
                }

                if (plugin.Type == Models.Entities.PluginType.Compiled && _hostPluginsCompiled.TryGetValue(plugin.Name, out var pluginType))
                {
                    var kernelPluginCompiled = CreateKernelPluginCompiled(tempServiceProvider, pluginType, plugin.Name, credentialService);

                    agentPlugins.Add(kernelPluginCompiled);
                }
                else if (plugin.Type == Models.Entities.PluginType.Curated && _hostPluginsCurated.TryGetValue(plugin.Name, out var kernelPluginCurated))
                {
                    agentPlugins.Add(kernelPluginCurated);
                }
                else if (plugin.Type == Models.Entities.PluginType.Curated)
                {
                    // We can still load the plugin if it's curated and not found in the host plugins
                    agentPlugins.Add(CreateKernelPluginCurated(plugin));
                }
            }

            if (!string.IsNullOrWhiteSpace(modelAgent.ChatCompletionFunctionName))
            {
                var (pluginName, functionName) = modelAgent.ChatCompletionFunctionName.Split('.') switch
                {
                    var parts when parts.Length == 2 => (parts[0], parts[1]),
                    _ => throw new InvalidOperationException($"Invalid ChatCompletionFunctionName format: {modelAgent.ChatCompletionFunctionName}")
                };

                if (functionName.EndsWith("Async"))
                {
                    functionName = functionName.Substring(0, functionName.Length - "Async".Length);
                }

                var chatCompletionPlugins = new KernelPluginCollection();

                KernelPlugin? kernelPlugin = null;

                if (_hostPluginsCompiled.TryGetValue(pluginName, out var pluginType) || _hostPluginsCurated.TryGetValue(pluginName, out kernelPlugin))
                {
                    kernelPlugin = CreateKernelPluginCompiled(tempServiceProvider, pluginType!, pluginName, credentialService);

                    if (kernelPlugin.TryGetFunction(functionName, out var kernelFunction))
                    {
                        chatCompletionPlugins.AddFromFunctions(pluginName, new[] { kernelFunction });
                    }
                }
                else
                {
                    _logger.LogWarning($"Plugin {pluginName} not found. Chat Completion function {modelAgent.ChatCompletionFunctionName} will not be loaded.");
                }

                var chatCompletionFunction = chatCompletionPlugins.GetFunction(pluginName, functionName);

                kernelServiceCollection.AddScoped<IChatCompletionService>(sp => new AgienceChatCompletionService(chatCompletionFunction));
            }

            return CreateScopedAgent(modelAgent, kernelServiceCollection.BuildServiceProvider(), agentPlugins);

        }

        private Agent CreateScopedAgent(Models.Entities.Agent modelAgent, IServiceProvider serviceProvider, KernelPluginCollection plugins)
        {
            var kernel = new Kernel(serviceProvider, plugins);

            var agencyLogger = kernel.LoggerFactory.CreateLogger<Agency>();

            var agentLogger = kernel.LoggerFactory.CreateLogger<Agent>();
            
            var agency = GetAgency(modelAgent.Agency, agencyLogger);

            var agent = new Agent(modelAgent.Id, modelAgent.Name, _authority, _broker, agency, modelAgent.Persona, kernel, agentLogger);

            agency.AddLocalAgent(agent);

            _agents.Add(agent);

            return agent;
        }

        public void Dispose()
        {
            foreach (var agent in _agents)
            {
                agent.Dispose();
            }
        }

        private KernelPlugin CreateKernelPluginCompiled(IServiceProvider serviceProvider, Type pluginType, string pluginName, AgienceCredentialService credentialService)
        {

            {
                var pluginInstance = ActivatorUtilities.CreateInstance(serviceProvider, pluginType);

                return KernelPluginFactory.CreateFromObject(pluginInstance, pluginName);
            }

            /*
            var connectionAttribute = pluginType.GetCustomAttribute<PluginConnectionAttribute>();
            
            if (connectionAttribute != null)
            {
                // Could potentially do some binding or injection here
            }
            */
        }

        private KernelPlugin CreateKernelPluginCurated(Models.Entities.Plugin plugin)
        {
            var functions = new List<KernelFunction>();

            foreach (var function in plugin.Functions)
            {
                functions.Add(KernelFunctionFactory.CreateFromPrompt(function.Prompt!, null as PromptExecutionSettings, function.Name, function.Description, null, null));
            }
            return KernelPluginFactory.CreateFromFunctions(plugin.Name, functions);
        }

        internal Agency GetAgency(Models.Entities.Agency modelAgency, ILogger<Agency> logger)
        {
            if (!_agencies.TryGetValue(modelAgency.Id, out var agency))
            {
                agency = new Agency(_authority, _broker, logger)
                {
                    Id = modelAgency.Id,
                    Name = modelAgency.Name
                    // Could use AutoMapper here
                };

                _agencies[modelAgency.Id] = agency;
            }

            return _agencies[modelAgency.Id];
        }
    }
}