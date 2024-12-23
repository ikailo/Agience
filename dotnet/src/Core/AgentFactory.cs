using Agience.Core.Logging;
using Agience.Core.Models.Enums;
using Agience.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;

namespace Agience.Core
{
    internal class AgentFactory : IDisposable
    {
        private readonly IServiceProvider _mainServiceProvider;
        private readonly ILogger<AgentFactory> _logger;
        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly List<Agent> _agents = new();

        internal AgentFactory(
            IServiceProvider mainServiceProvider,
            Broker broker,
            Authority authority,
            ILogger<AgentFactory> logger
        )
        {
            _mainServiceProvider = mainServiceProvider.CreateScope().ServiceProvider;
            _broker = broker;
            _authority = authority;
            _logger = logger;
        }

        internal Agent CreateAgent(Models.Entities.Agent modelAgent)
        {
            var kernelServiceProvider = new ExtendedServiceProvider(_mainServiceProvider);
            var host = kernelServiceProvider.GetRequiredService<Host>();

            ConfigureKernelServices(kernelServiceProvider, modelAgent);

            AddExecutiveFunction(kernelServiceProvider, host, modelAgent);

            var agentPlugins = new KernelPluginCollection();

            // Create Kernel and Agent first
            var kernel = new Kernel(kernelServiceProvider, agentPlugins);
            kernel.Data["agent_id"] = modelAgent.Id;

            kernelServiceProvider.Services.AddSingleton(kernel);

            var agentLogger = kernel.LoggerFactory.CreateLogger<Agent>();
            var agent = new Agent(modelAgent.Id, modelAgent.Name, _authority, _broker, modelAgent.Persona, kernel, agentLogger);
            kernelServiceProvider.Services.AddSingleton(agent);

            _agents.Add(agent);

            // Add Plugins (deferred)
            InitializePlugins(host, kernelServiceProvider, agentPlugins, modelAgent);

            return agent;
        }

        private void ConfigureKernelServices(ExtendedServiceProvider kernelServiceProvider, Models.Entities.Agent modelAgent)
        {
            kernelServiceProvider.Services.AddSingleton<ILoggerFactory>(sp =>
            {
                var agienceEventLoggerFactory = new EventLoggerFactory(modelAgent.Id);
                var agienceEventLoggerProvider = _mainServiceProvider.GetRequiredService<EventLoggerProvider>();

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

            kernelServiceProvider.Services.AddSingleton(new AgienceCredentialService(modelAgent.Id, _authority, _broker));            
        }

        private void InitializePlugins(Host host, ExtendedServiceProvider serviceProvider, KernelPluginCollection agentPlugins, Models.Entities.Agent modelAgent)
        {
            foreach (var plugin in modelAgent.Plugins)
            {
                if (string.IsNullOrWhiteSpace(plugin.Name))
                {
                    _logger.LogWarning("Plugin name is empty.");
                    continue;
                }

                try
                {
                    if (plugin.PluginProvider == PluginProvider.SKPlugin)
                    {
                        var pluginType = host.Plugins.FirstOrDefault(p => p.Name == plugin.Name)?.Type;
                        if (pluginType != null)
                        {
                            if (host.PluginInstances.ContainsKey(pluginType.FullName ?? pluginType.Name))
                            {
                                var pluginName = pluginType.FullName ?? pluginType.Name;
                                var pluginInstance = host.PluginInstances[pluginName];
                                var kernelPlugin = CreateKernelPluginCompiled(pluginInstance, plugin.Name);
                                agentPlugins.Add(kernelPlugin);
                            }
                            else
                            {
                                var kernelPlugin = CreateKernelPluginCompiled(serviceProvider, pluginType, plugin.Name);
                                agentPlugins.Add(kernelPlugin);
                            }
                        }
                    }
                    else if (plugin.PluginProvider == PluginProvider.Prompt)
                    {
                        var promptPlugin = CreateKernelPluginPrompt(plugin);
                        agentPlugins.Add(promptPlugin);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to initialize plugin: {plugin.Name}");
                }
            }            
        }

        private void AddExecutiveFunction(ExtendedServiceProvider serviceProvider, Host host, Models.Entities.Agent modelAgent)
        {
            if (!string.IsNullOrWhiteSpace(modelAgent.ExecutiveFunctionId))
            {
                string executiveFunctionName = string.Empty;
                Func<IServiceProvider, IChatCompletionService> factory = null;

                foreach (var hostPlugin in host.Plugins)
                {
                    if (hostPlugin.Functions.Any(f => f.Id == modelAgent.ExecutiveFunctionId))
                    {
                        if (hostPlugin.PluginProvider == PluginProvider.SKPlugin && hostPlugin.Type != null)
                        {
                            var pluginType = hostPlugin.Type;
                            var pluginName = hostPlugin.Name;
                            executiveFunctionName = hostPlugin.Functions
                                .FirstOrDefault(f => f.Id == modelAgent.ExecutiveFunctionId)?.Name.Replace("Async", "");

                            factory = sp =>
                            {
                                var kernelPlugin = CreateKernelPluginCompiled(serviceProvider, pluginType, pluginName);
                                if (kernelPlugin.TryGetFunction(executiveFunctionName, out var executiveFunction))
                                {
                                    return new AgienceChatCompletionService(executiveFunction);
                                }
                                throw new InvalidOperationException($"Executive function '{executiveFunctionName}' could not be found.");
                            };
                        }
                        else if (hostPlugin.PluginProvider == PluginProvider.Prompt)
                        {
                            var plugin = CreateKernelPluginPrompt(hostPlugin);
                            executiveFunctionName = hostPlugin.Functions
                                .FirstOrDefault(f => f.Id == modelAgent.ExecutiveFunctionId)?.Name.Replace("Async", "");

                            factory = sp =>
                            {
                                if (plugin.TryGetFunction(executiveFunctionName, out var executiveFunction))
                                {
                                    return new AgienceChatCompletionService(executiveFunction);
                                }
                                throw new InvalidOperationException($"Executive function '{executiveFunctionName}' could not be found.");
                            };
                        }
                    }
                }

                if (factory != null)
                {
                    serviceProvider.Services.AddScoped<IChatCompletionService>(factory);
                }
                else
                {
                    _logger.LogWarning($"Could not find a plugin with the executive function id {modelAgent.ExecutiveFunctionId}");
                }
            }
        }

        private KernelPlugin CreateKernelPluginCompiled(ExtendedServiceProvider serviceProvider, Type pluginType, string pluginName)
        {
            var pluginInstance = ActivatorUtilities.CreateInstance(serviceProvider, pluginType);
            return KernelPluginFactory.CreateFromObject(pluginInstance, pluginName);
        }

        private KernelPlugin CreateKernelPluginCompiled<T>(T pluginInstance, string pluginName) where T : class
        {   
            return KernelPluginFactory.CreateFromObject(pluginInstance, pluginName);
        }

        private KernelPlugin CreateKernelPluginPrompt(Models.Entities.Plugin plugin)
        {
            var functions = plugin.Functions.Select(f => KernelFunctionFactory.CreateFromPrompt(f.Instruction, null as PromptExecutionSettings, f.Name, f.Description, null, null, null)).ToList();
            return KernelPluginFactory.CreateFromFunctions(plugin.Name, functions);
        }

        public void DisposeAgent(string agentId)
        {
            var agent = _agents.FirstOrDefault(a => a.Id == agentId);

            if (agent != null)
            {
                _agents.Remove(agent);
                //_mainServiceProvider.GetRequiredService<IKernelStore>().RemoveKernel(agent.Id);
                agent.Dispose();
            }
        }

        public void Dispose()
        {
            foreach (var agent in _agents.ToList())
            {
                DisposeAgent(agent.Id);
            }
            _agents.Clear();
        }
    }
}
