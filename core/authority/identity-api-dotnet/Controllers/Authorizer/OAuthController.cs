using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using Agience.Authority.Identity.Validators;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Duende.IdentityServer.Models;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Agience.Authority.Identity.Filters;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("oauth")]        
    [SecurityHeaders]
    [Authorize]
    public class OAuthController : ControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly StateValidator _stateValidator;
        private readonly AppConfig _appConfig;
        private readonly ILogger<OAuthController> _logger;

        public OAuthController(
            RecordsRepository repository,
            IHttpClientFactory httpClientFactory,
            StateValidator stateValidator,
            AppConfig appConfig,
            ILogger<OAuthController> logger
            )
        {
            _repository = repository;
            _httpClientFactory = httpClientFactory;
            _stateValidator = stateValidator;
            _appConfig = appConfig;
            _logger = logger;
        }

        protected string PersonId
        {
            get
            {
                return HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == "sub")?.Value
                    ?? throw new HttpRequestException("No personId found.");
            }
        }

        [HttpGet("credential/{credentialId}/start")]
        public async Task<IActionResult> StartOAuthFlow(string credentialId, [FromQuery] string returnUrl)
        {
            var personId = PersonId;

            var credential = await _repository.GetChildRecordByIdAsPersonAsync<Agent, Credential>("AgentId", credentialId, personId);

            if (credential == null)
            {
                return NotFound("Credential not found.");
            }

            if (credential.AuthorizerId == null)
            {
                return BadRequest("Credential does not have an authorizer.");
            }

            // Fetch the authorizer record
            var authorizer = await _repository.GetRecordByIdAsSystemAsync<Authorizer>(credential.AuthorizerId);
            if (authorizer == null)
            {
                return NotFound("Authorizer not found.");
            }

            // Construct the authorization URL
            var clientId = authorizer.ClientId;
            var redirectUri = $"{_appConfig.IssuerUri.AbsoluteUri}{authorizer.RedirectUriPath}";
            var authorizationEndpoint = authorizer.AuthUri;
            var scope = authorizer.Scopes;

            var state = await _stateValidator.GenerateStateAsync(PersonId, new Dictionary<string, string>
            {
                { "credentialId", credentialId },
                { "returnUrl", returnUrl }
            });

            var authUrl = $"{authorizationEndpoint}?response_type=code" +
              $"&client_id={Uri.EscapeDataString(clientId)}" +
              $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
              $"&scope={Uri.EscapeDataString(scope)}" +
              $"&state={Uri.EscapeDataString(state)}" +
              $"&prompt=consent" +
              $"&access_type=offline";

            // TODO: access_type and prompt are for Google only. Need to differentiate between providers.}";

            return Redirect(authUrl);
        }

        [HttpGet("authorizer/{authorizerId}/callback")]
        public async Task<IActionResult> HandleCallback(string authorizerId, [FromQuery] string code, [FromQuery] string state)
        {
            if (string.IsNullOrEmpty(state))
            {
                return BadRequest("Missing state parameter.");
            }

            // Validate the state parameter
            ClaimsPrincipal validatedState;

            try
            {
                validatedState = await _stateValidator.ValidateState(state, PersonId);
            }
            catch (SecurityTokenValidationException ex)
            {
                _logger.LogError(ex, "State validation failed.");
                return BadRequest("Invalid or expired state.");
            }

            // Extract required data from state claims
            var credentialId = validatedState.FindFirst("credentialId")?.Value;
            var returnUrl = validatedState.FindFirst("returnUrl")?.Value;

            if (string.IsNullOrEmpty(credentialId) || string.IsNullOrEmpty(returnUrl))
            {
                return BadRequest("State parameter is missing required data.");
            }

            // Fetch the authorizer record
            var authorizer = await _repository.GetRecordByIdAsSystemAsync<Authorizer>(authorizerId);
            if (authorizer == null)
            {
                return NotFound("Authorizer not found.");
            }

            // Exchange the authorization code for tokens
            var clientId = authorizer.ClientId;
            var clientSecret = authorizer.ClientSecret;
            var tokenEndpoint = authorizer.TokenUri;
            var redirectUri = $"{_appConfig.IssuerUri.AbsoluteUri}{authorizer.RedirectUriPath}";
            var client = _httpClientFactory.CreateClient();

            string content =               
              $"&client_id={Uri.EscapeDataString(clientId)}" +
              $"&client_secret={Uri.EscapeDataString(clientSecret)}" +              
              $"&code={Uri.EscapeDataString(code)}" +
              $"&grant_type={Uri.EscapeDataString("authorization_code")}" +
              $"&redirect_uri={Uri.EscapeDataString(redirectUri)}";            
            
            var tokenResponse = await client.PostAsync(tokenEndpoint, new FormUrlEncodedContent(new[]
            {                   
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("code", code),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("redirect_uri", redirectUri)
            }));

            if (!tokenResponse.IsSuccessStatusCode)
            {
                var errorContent = await tokenResponse.Content.ReadAsStringAsync();
                return BadRequest($"Token exchange failed: {errorContent}");
            }

            var tokens = await tokenResponse.Content.ReadFromJsonAsync<TokenResponse>();           

            // Fetch the credential using credentialId from state
            var credential = await _repository.GetRecordByIdAsSystemAsync<Credential>(credentialId);
            if (credential == null)
            {
                return NotFound("Credential not found.");
            }

            // Save tokens to the database
            if (tokens != null)
            {
                credential.AccessToken = tokens.access_token;
                credential.RefreshToken = tokens.refresh_token;
                credential.ExpiryDate = DateTime.UtcNow.AddSeconds(Convert.ToDouble(tokens.expires_in));

                credential.CreatedDate = null;

                await _repository.UpdateRecordAsSystemAsync(credential);
            }

            // Redirect back to the client application using returnUrl from the state
            return Redirect(returnUrl);
        }
    }

    public class TokenResponse
    {
        public string? access_token { get; set; }
        public string? refresh_token { get; set; }
        public int? expires_in { get; set; }
        public string? token_type { get; set; }
        public string? scope { get; set; }
    }
}