using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using Agience.Authority.Identity.Exceptions;
using Agience.Authority.Identity.Validators;
using Agience.Authority.Identity.Services;
using System.Text.Json;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class AgentController : ManageControllerBase
    {
        private readonly Core.Authority _authority;
        private readonly AppConfig _appConfig;
        private readonly IMapper _mapper;
        private readonly StateValidator _stateValidator;

        public AgentController(Core.Authority authority, AppConfig appconfig, IAgienceDataAdapter dataAdapter, StateValidator stateValidator, ILogger<AgentController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _authority = authority;
            _appConfig = appconfig;
            _stateValidator = stateValidator;
            _mapper = mapper;
        }

        [HttpGet("agents")]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<IEnumerable<Agent>>(await _dataAdapter.GetRecordsAsPersonAsync<Models.Agent>(PersonId));
            });
        }

        [HttpGet("agent/{id}")]
        public async Task<ActionResult<Agent>> GetAgent(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<Agent>(await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Agent>(id, PersonId));
            });
        }

        [HttpPost("agent")]
        public async Task<ActionResult> PostAgent([FromBody] Agent agent)
        {
            return await HandlePost(async () =>
                {
                    agent = await _dataAdapter.CreateRecordAsPersonAsync(_mapper.Map<Models.Agent>(agent), PersonId) ?? throw new InvalidOperationException("Agent not created.");

                    var createdAgent = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Agent>(agent.Id, PersonId);

                    if (createdAgent == null)
                    {
                        throw new InvalidOperationException("Agent not found after creation.");
                    }

                    await _authority.AgentCreated(_mapper.Map<Core.Models.Entities.Agent>(createdAgent));

                    return createdAgent;

                },
                nameof(GetAgent)
            );            
        }

        [HttpPut("agent")]
        public async Task<IActionResult> PutAgent([FromBody] Agent agent)
        {
            return await HandlePut(async () =>
            {

                _logger.LogDebug("agent: " + JsonSerializer.Serialize(agent));

                await _dataAdapter.UpdateRecordAsPersonAsync(_mapper.Map<Models.Agent>(agent), PersonId);

                var updatedAgent = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Agent>(agent.Id, PersonId);

                if (updatedAgent == null)
                {
                    throw new InvalidOperationException("Agent not found after creation.");
                }

                await _authority.AgentUpdated(_mapper.Map<Core.Models.Entities.Agent>(updatedAgent));
            });
        }

        [HttpDelete("agent/{id}")]
        public async Task<IActionResult> DeleteAgent(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Agent>(id, PersonId);

                await _authority.AgentDeleted(new Core.Models.Entities.Agent() {Id = id });
            });
        }

        // PLUGINS //

        [HttpPut("agent/{agentId}/plugin/{pluginId}")]
        public async Task<IActionResult> PutAgentPlugin(string agentId, string pluginId)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.AddPluginToAgentAsPersonAsync(agentId, pluginId, PersonId);
            });
        }

        [HttpDelete("agent/{agentId}/plugin/{pluginId}")]
        public async Task<IActionResult> DeleteAgentPlugin(string agentId, string pluginId)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.RemovePluginFromAgentAsPersonAsync(agentId, pluginId, PersonId);
            });
        }

        // CONNECTIONS //

        [HttpGet("agent/{id}/plugins/connections")]
        public async Task<ActionResult<IEnumerable<PluginConnection>>> GetAgentPluginsConnections(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<IEnumerable<PluginConnection>>(await _dataAdapter.GetPluginConnectionsForAgentAsPersonAsync(id, PersonId));
            });
        }

        [HttpPut("agent/{agentId}/connection/{pluginConnectionId}/authorizer/{authorizerId}")]
        public async Task<IActionResult> PutAgentConnectionAuthorizer(string agentId, string pluginConnectionId, string authorizerId)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.UpsertAgentConnectionAsPersonAsync(agentId, pluginConnectionId, authorizerId, null, PersonId);
            });
        }

        [HttpGet("agent/{agentId}/connection/{pluginConnectionId}/activate")]
        public async Task<IActionResult> ActivateConnection(string agentId, string pluginConnectionId)
        {
            return await HandleRedirect(async () =>
            {
                var agentConnection = await _dataAdapter.GetAgentConnectionAsPersonAsync(agentId, pluginConnectionId, PersonId)
                    ?? throw new NotFoundException("AgentConnection not found");

                if (string.IsNullOrWhiteSpace(agentConnection.AuthorizerId))
                {
                    throw new InvalidOperationException("AgentConnection does not specify an Authorizer");
                }

                var authorizer = await _dataAdapter.GetRecordByIdAsync<Models.Authorizer>(agentConnection.AuthorizerId)
                    ?? throw new NotFoundException("Authorizer not found");

                // Generate state parameter using StateValidator
                var state = await _stateValidator.GenerateStateAsync(PersonId, new Dictionary<string, string>
                {
                    { "plugin_connection_id", pluginConnectionId },
                    { "agent_id", agentId }
                });

                return authorizer.GetAuthorizationUri(_appConfig.AuthorityUri, state);
            });
        }

        // TODO: Deactivate Connection

        // Functions

        [HttpGet("agent/{id}/functions")]
        public async Task<ActionResult<IEnumerable<Function>>> GetAgentFunctions(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<IEnumerable<Function>>(await _dataAdapter.GetFunctionsForAgentAsPersonAsync(id, PersonId));
            });
        }
    }
}
