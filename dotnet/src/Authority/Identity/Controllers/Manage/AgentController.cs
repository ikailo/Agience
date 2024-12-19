using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using LinqKit;
using Sprache;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [ApiController]
    [Route("manage")]
    public class AgentController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public AgentController(ILogger<AgentController> logger, RecordsRepository repository, IMapper mapper) : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // *** AGENT *** //

        [HttpPost("agent")]
        public async Task<ActionResult<ManageModel.Agent>> CreateAgent([FromBody] Agent agent)
        {
            return await HandlePost(async () =>
            {
                agent = await _repository.CreateRecordAsPersonAsync(agent, PersonId);

                if (agent == null)
                    throw new InvalidOperationException("Agent could not be created.");

                return _mapper.Map<ManageModel.Agent>(agent);
            }, nameof(GetAgentById), "agentId");
        }

        [HttpGet("agents")]
        public async Task<ActionResult<IEnumerable<ManageModel.Agent>>> GetAgents([FromQuery] string? search = null)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var agents = await _repository.GetRecordsAsPersonAsync<Agent>(PersonId);
                    return _mapper.Map<IEnumerable<ManageModel.Agent>>(agents);
                }

                var searchResults = await _repository.SearchRecordsAsPersonAsync<Agent>(
                    new[] { "Name", "Description" },
                    search,
                    PersonId
                );

                return _mapper.Map<IEnumerable<ManageModel.Agent>>(searchResults);
            });
        }

        [HttpGet("agent/{agentId}")]
        public async Task<ActionResult<ManageModel.Agent>> GetAgentById(string agentId)
        {
            return await HandleGet(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                return _mapper.Map<ManageModel.Agent>(agent);
            });
        }

        [HttpPut("agent/{agentId}")]
        public async Task<IActionResult> UpdateAgent(string agentId, [FromBody] Agent agent)
        {
            return await HandlePut(async () =>
            {
                if (agent?.Id == null)
                    throw new ArgumentNullException("Agent Id is required.");

                if (agent.Id != null && !agent.Id.Equals(agentId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                agent.Id = agentId;

                await _repository.UpdateRecordAsPersonAsync(agent, PersonId);
            });
        }

        [HttpDelete("agent/{agentId}")]
        public async Task<IActionResult> DeleteAgent(string agentId)
        {
            return await HandleDelete(async () =>
            {
                var success = await _repository.DeleteRecordAsPersonAsync<Agent>(agentId, PersonId);

                if (!success)
                    throw new KeyNotFoundException("Agent not found or could not be deleted.");
            });
        }

        // *** AGENT_PLUGIN *** //

        [HttpGet("agent/{agentId}/plugins")]
        public async Task<ActionResult<IEnumerable<ManageModel.Plugin>>> GetPluginsForAgent(string agentId)
        {
            return await HandleGet(async () =>
            {
                var plugins = await _repository.GetChildRecordsWithJoinAsPersonAsync<Agent, Plugin, AgentPlugin>(
                    "AgentId",
                    "PluginId",
                    agentId,
                    PersonId
                );
                return _mapper.Map<IEnumerable<ManageModel.Plugin>>(plugins);
            });
        }

        [HttpPost("agent/{agentId}/plugin/{pluginId}")]
        public async Task<IActionResult> AddPluginToAgent(string agentId, string pluginId, bool all = false)
        {
            return await HandlePut(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId, all);

                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                var existingLink = await _repository.QueryRecordsAsSystemAsync<AgentPlugin>(
                    new Dictionary<string, object>
                    {
                        { "AgentId", agentId },
                        { "PluginId", pluginId }
                    });

                if (existingLink.Any())
                    throw new InvalidOperationException("Plugin is already associated with the Agent.");

                await _repository.CreateRecordAsSystemAsync(new AgentPlugin
                {
                    AgentId = agentId,
                    PluginId = pluginId
                });
            });
        }

        [HttpDelete("agent/{agentId}/plugin/{pluginId}")]
        public async Task<IActionResult> RemovePluginFromAgent(string agentId, string pluginId)
        {
            return await HandleDelete(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                var agentPlugins = await _repository.QueryRecordsAsSystemAsync<AgentPlugin>(
                    new Dictionary<string, object>
                    {
                        { "AgentId", agentId },
                        { "PluginId", pluginId }
                    });

                if (!agentPlugins.Any())
                    throw new InvalidOperationException("Agent is not associated with the Agent.");

                await _repository.DeleteRecordAsSystemAsync<AgentPlugin>(agentPlugins.First().Id!);
            });
        }

        // *** AGENT_TOPIC *** //

        [HttpPost("agent/{agentIdId}/topic")]
        public async Task<ActionResult<ManageModel.Topic>> CreateTopicForAgent(string agentId, [FromBody] Topic topic)
        {
            return await HandlePost(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                topic = await _repository.CreateRecordAsSystemAsync(topic);

                var agentTopic = await _repository.CreateRecordAsSystemAsync(new AgentTopic()
                {
                    AgentId = agentId,
                    TopicId = topic.Id,
                });

                return _mapper.Map<ManageModel.Topic>(topic);

            }, nameof(TopicController.GetTopicById), "topicId");
        }

        [HttpGet("agent/{agentId}/topics")]
        public async Task<ActionResult<IEnumerable<ManageModel.Topic>>> GetTopicsForAgent(string agentId, [FromQuery] bool all)
        {
            return await HandleGet(async () =>
            {
                var topics = await _repository.GetChildRecordsWithJoinAsPersonAsync<Agent, Topic, AgentTopic>("AgentId", "TopicId", agentId, PersonId, all);

                return _mapper.Map<IEnumerable<ManageModel.Topic>>(topics);
            });
        }

        [HttpPost("agent/{agentId}/topic/{topicId}")]
        public async Task<IActionResult> AddTopicToAgent(string agentId, string topicId, [FromQuery] bool all)
        {
            return await HandlePut(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                var topic = await _repository.GetRecordByIdAsPersonAsync<Topic>(topicId, PersonId, all);

                if (topic == null)
                    throw new KeyNotFoundException("Topic not found.");

                var agentTopics = await _repository.QueryRecordsAsSystemAsync<AgentTopic>(new() {
                    { "AgentId", agentId },
                    { "TopicId", topicId }
                });

                if (agentTopics.Count() != 0)
                    throw new InvalidOperationException("Topic is already associated with Agent.");

                await _repository.CreateRecordAsSystemAsync<AgentTopic>(new() { AgentId = agentId, TopicId = topicId });

            });
        }

        [HttpDelete("agent/{agentId}/topic/{topicId}")]
        public async Task<IActionResult> RemoveTopicFromAgent(string agentId, string topicId)
        {
            return await HandleDelete(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                var agentTopics = await _repository.QueryRecordsAsSystemAsync<AgentTopic>(new() {
                    { "AgentId", agentId },
                    { "TopicId", topicId }
                });

                if (agentTopics.Count() == 0)
                    throw new InvalidOperationException("Topic is not associated with Agent.");

                await _repository.DeleteRecordAsSystemAsync<AgentTopic>(agentTopics.First().Id!);

            });
        }

        // *** CREDENTIALS *** //

        [HttpPost("agent/{agentId}/credential")]
        [HttpPost("agent/{agentId}/credential/{credentialId}")]
        public async Task<ActionResult<ManageModel.Credential>> UpsertCredentialForAgent(
     string agentId, string? credentialId, [FromBody] Credential credential, bool all = false)
        {
            return await HandlePost(async () =>
            {
                // Validation: Check for conflicts in CredentialId
                if (credentialId == null && credential.Id != null)
                    throw new InvalidOperationException("Credential.Id not allowed in a create call.");

                if (credential.Id != null && credentialId != null && credential.Id != credentialId)
                    throw new InvalidOperationException("If CredentialId is provided in the body, it must match the URL.");

                if (credential.AgentId != null && credential.AgentId != agentId)
                    throw new InvalidOperationException("If AgentId is provided in the body, it must match the URL.");

                // Validate Agent existence
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);
                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                // Validate Connection existence
                if (credential.ConnectionId != null)
                {
                    var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(credential.ConnectionId, PersonId, all);
                    if (connection == null)
                        throw new KeyNotFoundException("Connection not found.");
                }

                // Check for existing credential for Agent and Connection
                Credential? existingCredential = null;
                if (credential.ConnectionId != null)
                {
                    existingCredential = (await _repository.QueryRecordsAsSystemAsync<Credential>(
                        new Dictionary<string, object> { { "AgentId", agentId }, { "ConnectionId", credential.ConnectionId } }
                    )).FirstOrDefault();
                }

                if (existingCredential != null)
                {
                    // Update the existing credential instead of throwing an error
                    existingCredential.AccessToken = credential.AccessToken ?? existingCredential.AccessToken;
                    existingCredential.RefreshToken = credential.RefreshToken ?? existingCredential.RefreshToken;
                    existingCredential.ConnectionId = credential.ConnectionId;
                    existingCredential.AgentId = agentId;
                    existingCredential.CreatedDate = null;

                    var updatedResult = await _repository.UpdateRecordAsSystemAsync(existingCredential);

                    var responseCredential = new Credential
                    {
                        Id = updatedResult.Id,
                        ConnectionId = updatedResult.ConnectionId,
                        AgentId = updatedResult.AgentId,
                        HasAccessToken = !string.IsNullOrEmpty(updatedResult.AccessToken),
                        HasRefreshToken = !string.IsNullOrEmpty(updatedResult.RefreshToken),
                        AccessToken = null,
                        RefreshToken = null
                    };

                    return _mapper.Map<ManageModel.Credential>(responseCredential);
                }

                if (credentialId != null)
                {
                    // Update credential by ID if it exists
                    var credentialToUpdate = await _repository.GetRecordByIdAsSystemAsync<Credential>(credentialId);
                    if (credentialToUpdate == null)
                        throw new KeyNotFoundException("Credential not found.");

                    credentialToUpdate.AccessToken = credential.AccessToken ?? credentialToUpdate.AccessToken;
                    credentialToUpdate.RefreshToken = credential.RefreshToken ?? credentialToUpdate.RefreshToken;
                    credentialToUpdate.ConnectionId = credential.ConnectionId;
                    credentialToUpdate.AgentId = agentId;

                    var updatedResult = await _repository.UpdateRecordAsSystemAsync(credentialToUpdate);

                    return _mapper.Map<ManageModel.Credential>(updatedResult);
                }

                // Create a new credential
                credential.Id = (await _repository.CreateRecordAsSystemAsync(credential))?.Id
                    ?? throw new InvalidOperationException("Unable to create credential");

                credential.AccessToken = null; // Mask sensitive data
                credential.RefreshToken = null;

                return _mapper.Map<ManageModel.Credential>(credential);

            }, nameof(GetCredentialById), "credentialId");
        }



        [HttpGet("agent/{agentId}/credentials")]
        public async Task<ActionResult<IEnumerable<ManageModel.Credential>>> GetCredentialsForAgent(string agentId)
        {
            return await HandleGet(async () =>
            {
                // Step 1: Fetch committed credentials for the agent
                var committedCredentials = await _repository.GetChildRecordsAsPersonAsync<Agent, Credential>(
                    "AgentId", agentId, PersonId
                );

                // Create a lookup dictionary for quick access
                var committedCredentialsLookup = committedCredentials.ToDictionary(c => c.ConnectionId, c => c);

                // Step 2: Fetch all plugin IDs associated with the agent
                var pluginIds = (await _repository.GetChildRecordsWithJoinAsPersonAsync<Agent, Plugin, AgentPlugin>(
                    "AgentId", "PluginId", agentId, PersonId
                ))?.Select(p => p.Id) ?? Enumerable.Empty<string>();

                // Step 3: Fetch all function IDs associated with the plugins
                var functionIds = (await _repository.GetChildRecordsByIdsWithJoinAsSystemAsync<Plugin, Function, PluginFunction>(
                    "PluginId", "FunctionId", pluginIds
                ))?.Select(f => f.Id) ?? Enumerable.Empty<string>();

                // Step 4: Fetch all connections associated with the functions
                var allConnections = await _repository.GetChildRecordsByIdsWithJoinAsSystemAsync<Function, Connection, FunctionConnection>(
                    "FunctionId", "ConnectionId", functionIds
                ) ?? Enumerable.Empty<Connection>();

                // Step 5: Merge committed credentials with placeholders
                var allCredentials = allConnections.Select(connection =>
                {
                    return committedCredentialsLookup.TryGetValue(connection.Id, out var existingCredential)
                        ? existingCredential
                        : new Credential
                        {
                            Connection = connection,
                            ConnectionId = connection.Id,
                            AgentId = agentId
                        };
                }).ToList();

                // Step 6: Fetch executive credentials if ExecutiveFunctionId exists
                var agent = await _repository.GetRecordByIdAsSystemAsync<Agent>(agentId);
                if (agent?.ExecutiveFunctionId != null)
                {
                    var executiveConnections = await _repository.GetChildRecordsByIdsWithJoinAsSystemAsync<Function, Connection, FunctionConnection>(
                        "FunctionId", "ConnectionId", new List<string> { agent.ExecutiveFunctionId }
                    ) ?? Enumerable.Empty<Connection>();

                    foreach (var executiveConnection in executiveConnections)
                    {
                        if (!allCredentials.Any(c => c.ConnectionId == executiveConnection.Id))
                        {
                            // Fetch the existing executive credential for the connection
                            var executiveCredential = (await _repository.QueryRecordsAsSystemAsync<Credential>(
                                   new Dictionary<string, object> { { "AgentId", agentId }, { "ConnectionId", executiveConnection.Id } }
                               )).FirstOrDefault();

                            var newCredential = executiveCredential ?? new Credential
                            {
                                Connection = executiveConnection,
                                ConnectionId = executiveConnection.Id,
                                AgentId = agentId
                            };

                            allCredentials.Add(newCredential);
                        }
                    }
                }

                // Step 7: Set token flags and nullify sensitive data
                allCredentials.ForEach(c =>
                {
                    c.HasAccessToken = !string.IsNullOrEmpty(c.AccessToken);
                    c.HasRefreshToken = !string.IsNullOrEmpty(c.RefreshToken);
                    c.AccessToken = null;
                    c.RefreshToken = null;
                });

                // Map and return the results
                return _mapper.Map<IEnumerable<ManageModel.Credential>>(allCredentials);
            });
        }


        [HttpGet("agent/credential/{credentialId}")]
        public async Task<ActionResult<IEnumerable<ManageModel.Credential>>> GetCredentialById(string credentialId)
        {
            return await HandleGet(async () =>
            {
                var credential = await _repository.GetChildRecordByIdAsPersonAsync<Agent, Credential>("AgentId", credentialId, PersonId);

                credential.HasAccessToken = !string.IsNullOrEmpty(credential.AccessToken);
                credential.HasRefreshToken = !string.IsNullOrEmpty(credential.RefreshToken);

                credential.RefreshToken = null;
                credential.AccessToken = null;

                return _mapper.Map<IEnumerable<ManageModel.Credential>>(credential);
            });
        }

        [HttpDelete("agent/credential/{credentialId}")]
        public async Task<IActionResult> ClearCredentialById(string credentialId)
        {
            return await HandleDelete(async () =>
            {
                var credential = await _repository.GetChildRecordByIdAsPersonAsync<Agent, Credential>("AgentId", credentialId, PersonId);

                if (credential == null)
                    throw new KeyNotFoundException("Credential not found.");

                await _repository.DeleteRecordAsSystemAsync<Credential>(credentialId);
            });
        }

        // *** AGENT_LOG_ENTRY *** //

        [HttpPost("agent/{agentId}/log")]
        public async Task<ActionResult<ManageModel.AgentLogEntry>> CreateLogEntryForAgent(string agentId, [FromBody] AgentLogEntry log)
        {
            return await HandlePost(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                {
                    throw new KeyNotFoundException("Agent not found.");
                }

                if (log?.AgentId != agentId) throw new InvalidDataException("If AgentId is provided in log, it must match AgentId provided in URL.");

                log.AgentId = agentId;

                // Create the log entry
                var createdLog = await _repository.CreateRecordAsSystemAsync(log);

                return _mapper.Map<ManageModel.AgentLogEntry>(createdLog);

            }, nameof(GetLogEntryById), "logEntryId");
        }

        [HttpGet("agent/{agentId}/logs")]
        public async Task<ActionResult<IEnumerable<ManageModel.AgentLogEntry>>> GetLogEntriesForAgent(string agentId)
        {
            return await HandleGet(async () =>
            {
                var agentLogEntries = _repository.GetChildRecordsAsPersonAsync<Agent, AgentLogEntry>("AgentId", agentId, PersonId);
                return _mapper.Map<IEnumerable<ManageModel.AgentLogEntry>>(agentLogEntries);
            });
        }

        [HttpGet("agent/log/{logEntryId}")]
        public async Task<ActionResult<ManageModel.AgentLogEntry>> GetLogEntryById(string logEntryId)
        {
            return await HandleGet(async () =>
            {
                var agentLogEntry = _repository.GetChildRecordByIdAsPersonAsync<Agent, AgentLogEntry>("AgentId", logEntryId, PersonId);
                return _mapper.Map<ManageModel.AgentLogEntry>(agentLogEntry);
            });
        }
    }
}