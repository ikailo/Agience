using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using Agience.Authority.Identity.Exceptions;
using System.Security.Claims;
using Agience.Authority.Identity.Validators;
using Duende.IdentityServer.Extensions;
using Newtonsoft.Json;

namespace Agience.Authority.Identity.Controllers.Manage
{
    // This does not require Authentication - it is the responsibility of the handler to validate the state.

    [Route("manage")]
    [ApiController]
    public class CallbackController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IAgienceDataAdapter _dataAdapter;
        private readonly ILogger<CallbackController> _logger;
        private readonly StateValidator _stateValidator;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppConfig _appConfig;

        public CallbackController(IAgienceDataAdapter dataAdapter, StateValidator stateValidator, ILogger<CallbackController> logger, IMapper mapper, IHttpClientFactory httpClientFactory, AppConfig appConfig)
        {
            _dataAdapter = dataAdapter;
            _stateValidator = stateValidator;
            _logger = logger;
            _mapper = mapper;
            _httpClientFactory = httpClientFactory;
            _appConfig = appConfig;
        }

        [HttpGet("authorizer/{id}/callback")]
        public async Task<IActionResult> GetAuthorizerCallback(string id, [FromQuery] string? code, [FromQuery] string? state)
        {
            _logger.LogInformation($"Activating Authorizer {id} with code {code} and state {state}");

            if (string.IsNullOrEmpty(state))
            {
                return BadRequest("State parameter is missing.");
            }

            try
            {
                var personId = HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? HttpContext.User.GetSubjectId() ?? throw new InvalidOperationException("No personId found.");

                var claimsPrincipal = await _stateValidator.ValidateState(state, personId);

                var personIdClaim = claimsPrincipal.FindFirst("user_id")?.Value ?? throw new InvalidOperationException("user_id not found in state.");

                var authorizer = await _dataAdapter.GetRecordByIdAsync<Models.Authorizer>(id) ?? throw new NotFoundException("Authorizer not found");

                var agentId = claimsPrincipal.FindFirst("agent_id")?.Value ?? throw new InvalidOperationException("agent_id not found in state.");

                var pluginConnectionId = claimsPrincipal.FindFirst("plugin_connection_id")?.Value ?? throw new InvalidOperationException("plugin_connection_id not found in state.");

                var agentConnection = await _dataAdapter.GetAgentConnectionAsPersonAsync(agentId, pluginConnectionId, personId) ?? throw new NotFoundException("AgentConnection not found");

                var httpClient = _httpClientFactory.CreateClient();

                var response = await httpClient.PostAsync(authorizer.TokenUri,
                    new FormUrlEncodedContent(new Dictionary<string, string>
                        {
                            { "code", code },
                            { "client_id", authorizer.ClientId },
                            { "client_secret", authorizer.ClientSecret },
                            { "redirect_uri", $"{_appConfig.AuthorityUri}{authorizer.RedirectUri}" },
                            { "grant_type", "authorization_code" },
                        }
                    )
                );

                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to exchange code for token. Status code: {response.StatusCode}");
                    throw new InvalidOperationException("Failed to exchange code for token.");
                }                

                var responseContent = await response.Content.ReadAsStringAsync();
                var tokenResponse = JsonConvert.DeserializeObject<Dictionary<string, string>>(responseContent);

                if (tokenResponse != null && tokenResponse.TryGetValue("refresh_token", out var refreshToken))
                {
                    var credential = await _dataAdapter.CreateRecordAsPersonAsync(new Models.Credential { Secret = refreshToken }, personId) ?? throw new InvalidOperationException("Credential not created.");

                    agentConnection.CredentialId = credential.Id;
                    agentConnection.Status = ConnectionStatus.Active;

                    await _dataAdapter.UpdateRecordAsPersonAsync(agentConnection, personId);

                    return Ok();
                }

                throw new InvalidOperationException("Invalid token response.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "State validation failed.");

                return BadRequest("Invalid state parameter.");
            }
        }
    }
}
