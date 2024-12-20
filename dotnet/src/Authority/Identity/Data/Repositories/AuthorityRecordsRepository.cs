using Agience.Authority.Identity.Data.Adapters;
using Agience.Core.Interfaces;
using CoreModel = Agience.Core.Models.Entities;
using Agience.Authority.Identity.Models;
using Host = Agience.Authority.Identity.Models.Host;
using AutoMapper;
using System.Data.Entity.Migrations.Model;

namespace Agience.Authority.Identity.Data.Repositories
{
    public class AuthorityRecordsRepository : RecordsRepository, IAuthorityRecordsRepository
    {
        private readonly IMapper _mapper;

        public AuthorityRecordsRepository(AgienceDataAdapter dataAdapter, ILogger<AuthorityRecordsRepository> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {

            _mapper = mapper;
        }

        public async Task<string?> GetHostIdForAgentById(string agentId)
        {
            return (await GetRecordByIdAsSystemAsync<Agent>(agentId))?.HostId;
        }

        public async Task<IEnumerable<CoreModel.Agent>> GetAgentsForHostById(string hostId)
        {
            // Fetch Agents for the specified Host
            var agents = await GetChildRecordsAsSystemAsync<Host, Agent>("HostId", hostId);

            foreach (var agent in agents)
            {
                // Initialize the Plugins collection for the Agent
                agent.Plugins = new List<Plugin>();

                // Fetch AgentPlugins associated with the Agent
                var agentPlugins = await _dataAdapter.QueryRecordsAsync<AgentPlugin>(new Dictionary<string, object>
                {
                    { "AgentId", agent.Id }
                });

                // Fetch Plugins for each AgentPlugin
                foreach (var agentPlugin in agentPlugins)
                {
                    var plugin = await GetRecordByIdAsSystemAsync<Plugin>(agentPlugin.PluginId);
                    if (plugin != null && !agent.Plugins.Any(p => p.Id == plugin.Id))
                    {
                        agent.Plugins.Add(plugin);
                    }
                }
            }

            // Map the results to the CoreModel.Agent type
            return _mapper.Map<IEnumerable<CoreModel.Agent>>(agents);
        }



        public async Task<CoreModel.Host?> GetHostById(string hostId)
        {
            return await GetRecordByIdAsSystemAsync<Host>(hostId);
        }

        public async Task<IEnumerable<CoreModel.Plugin>> GetPluginsForHostById(string hostId)
        {
            // Fetch Plugins
            var plugins = await GetChildRecordsByIdsWithJoinAsSystemAsync<Host, Plugin, HostPlugin>("HostId", "PluginId", new[] { hostId });

            // Load Functions and Parameters for each Plugin
            foreach (var plugin in plugins)
            {
                // Fetch Plugin's Functions
                var pluginFunctions = await _dataAdapter.QueryRecordsAsync<PluginFunction>(new Dictionary<string, object>
                    {
                        { "PluginId", plugin.Id }
                    });

                // Fetch and attach Functions
                foreach (var pluginFunction in pluginFunctions)
                {
                    var function = await GetRecordByIdAsSystemAsync<Function>(pluginFunction.FunctionId);
                    if (function != null)
                    {
                        // Fetch Inputs and Outputs for the Function
                        var inputs = await _dataAdapter.QueryRecordsAsync<Parameter>(new Dictionary<string, object>
                            {
                                { "InputFunctionId", function.Id }
                            });

                        var outputs = await _dataAdapter.QueryRecordsAsync<Parameter>(new Dictionary<string, object>
                            {
                                { "OutputFunctionId", function.Id }
                            });

                        // Assign Inputs and Outputs to Function
                        function.Inputs = inputs.ToList();
                        function.Outputs = outputs.ToList();

                        // Attach Function to Plugin
                        plugin.Functions.Add(function);
                    }
                }
            }

            return _mapper.Map<IEnumerable<CoreModel.Plugin>>(plugins);
        }


        public async Task<IEnumerable<CoreModel.Plugin>> SyncPluginsForHostById(string hostId, List<CoreModel.Plugin> plugins)
        {
            var host = await GetHostById(hostId);

            // Step 1: Retrieve existing plugins and their relationships
            var existingPlugins = (await GetPluginsForHostById(hostId)).ToList();

            // Step 2: Create dictionaries for efficient lookups
            var existingPluginDict = existingPlugins.ToDictionary(p => p.UniqueName ?? string.Empty, p => p);
            var providedPluginNames = plugins.Select(p => p.UniqueName ?? string.Empty).ToHashSet();

            var pluginsToCreate = new List<CoreModel.Plugin>();

            // Step 4: Compare and sync plugins
            foreach (var plugin in plugins)
            {
                if (existingPluginDict.TryGetValue(plugin.UniqueName ?? string.Empty, out var existingPlugin))
                {
                    // Plugin exists; update it
                    //UpdatePlugin(existingPlugin, plugin);
                    //pluginsToUpdate.Add(existingPlugin);
                }
                else
                {
                    // Plugin does not exist; create it
                    pluginsToCreate.Add(plugin);
                }
            }

            // Handle plugin creation
            foreach (var plugin in pluginsToCreate)
            {
                plugin.OwnerId = host?.OwnerId;

                // Create the plugin and retrieve the generated ID
                var createdPlugin = await CreateRecordAsSystemAsync(_mapper.Map<Plugin>(plugin));

                // Create HostPlugin relationship
                var hostPlugin = new HostPlugin
                {
                    HostId = hostId,
                    PluginId = createdPlugin.Id
                };
                await CreateRecordAsSystemAsync(hostPlugin);

                // Create Functions and Parameters for the plugin
                foreach (var function in plugin.Functions)
                {
                    await CreateFunctionWithParameters(createdPlugin.Id, function);
                }
            }
            
            return await GetPluginsForHostById(hostId);
        }

        // Helper method to create a function with parameters
        private async Task CreateFunctionWithParameters(string pluginId, CoreModel.Function function)
        {
            // Create the function
            var createdFunction = await CreateRecordAsSystemAsync(_mapper.Map<Function>(function));

            // Create PluginFunction relationship
            var pluginFunction = new PluginFunction
            {
                PluginId = pluginId,
                FunctionId = createdFunction.Id
            };
            await CreateRecordAsSystemAsync(pluginFunction);

            // Create input parameters
            foreach (var input in function.Inputs)
            {
                input.InputFunctionId = createdFunction.Id;
                await CreateRecordAsSystemAsync(_mapper.Map<Parameter>(input));
            }

            // Create output parameters
            foreach (var output in function.Outputs)
            {
                output.OutputFunctionId = createdFunction.Id;
                await CreateRecordAsSystemAsync(_mapper.Map<Parameter>(output));
            }
        }

        // TODO: SECURITY: This is insecure. This will be fixed when we can store credentials encrypted at rest.
        public async Task<string?> GetCredentialForAgentByName(string agentId, string connectionName)
        {
            // Validate input
            if (string.IsNullOrWhiteSpace(agentId))
                throw new ArgumentException("Agent ID cannot be null or empty.", nameof(agentId));
            if (string.IsNullOrWhiteSpace(connectionName))
                throw new ArgumentException("Connection name cannot be null or empty.", nameof(connectionName));

            // Fetch the agent record
            var agent = await GetRecordByIdAsSystemAsync<Agent>(agentId);
            if (agent == null)
                throw new KeyNotFoundException($"Agent with ID '{agentId}' not found.");

            // Query the connection for the agent
            var connections = await _dataAdapter.QueryRecordsAsync<Connection>(new Dictionary<string, object>
            {
                { "OwnerId", agent.OwnerId },
                { "Name", connectionName }
            });

            foreach (var connection in connections)
            {
                // Fetch the credentials for the connection
                var credentials = await _dataAdapter.QueryRecordsAsync<Credential>(new Dictionary<string, object>
                {
                    { "ConnectionId", connection.Id },
                    { "AgentId", agentId }
                });
                // Return the first matching credential value
                return credentials.FirstOrDefault()?.AccessToken;
            }

            return null;
        }

    }
}
