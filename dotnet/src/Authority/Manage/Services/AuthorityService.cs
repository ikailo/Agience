using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.IdentityModel.Tokens;

namespace Agience.Authority.Manage.Services
{
    public class AuthorityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILogger<AuthorityService> _logger;
        private readonly AppConfig _appConfig;

        public string AuthorityUri => (_appConfig.AuthorityUriInternal ?? _appConfig.AuthorityUri)
            ?? throw new ArgumentNullException(nameof(_appConfig.AuthorityUri));

        public AuthorityService(
            IHttpClientFactory httpClientFactory,
            AuthenticationStateProvider authenticationStateProvider,
            ILogger<AuthorityService> logger,
            AppConfig appConfig
        )
        {
            _httpClientFactory = httpClientFactory;
            _authenticationStateProvider = authenticationStateProvider;
            _logger = logger;
            _appConfig = appConfig;
        }

        /// <summary>
        /// Asynchronously gets an HttpClient configured with an access token.
        /// </summary>
        public async Task<HttpClient?> GetHttpClientAsync()
        {
            try
            {
                var httpClient = _httpClientFactory.CreateClient("authority");
                httpClient.BaseAddress = new Uri(AuthorityUri);

                // Retrieve token from AuthenticationStateProvider
                var accessToken = await GetAccessTokenFromAuthenticationStateAsync();
                if (!string.IsNullOrEmpty(accessToken))
                {
                    _logger.LogDebug("Successfully retrieved access token.");
                    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                    return httpClient;
                }

                _logger.LogWarning("Access token is missing.");
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create and configure HttpClient.");
                return null;
            }
        }

        /// <summary>
        /// Retrieves the access token from the current authentication state.
        /// </summary>
        private async Task<string?> GetAccessTokenFromAuthenticationStateAsync()
        {
            try
            {
                var authState = await _authenticationStateProvider.GetAuthenticationStateAsync();
                var user = authState.User;

                if (user?.Identity?.IsAuthenticated == true)
                {
                    var accessToken = user.FindFirst("access_token")?.Value;
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        return accessToken;
                    }

                    _logger.LogWarning("No access token found in user claims.");
                }
                else
                {
                    _logger.LogWarning("User is not authenticated.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving access token.");
            }

            return null;
        }

        /// <summary>
        /// Validates the session by calling the /userinfo endpoint.
        /// </summary>
        public async Task<bool> CheckUserInfoEndpointAsync(string userIdentifier)
        {
            try
            {
                var token = await GetAccessTokenFromAuthenticationStateAsync();
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Access token is missing.");
                    return false;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{AuthorityUri}/connect/userinfo");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling /userinfo endpoint.");
                return false;
            }
        }

        /// <summary>
        /// Fetches signing keys from the authority's JWKS endpoint.
        /// </summary>
        public async Task<IEnumerable<SecurityKey>> FetchSigningKeysAsync()
        {
            try
            {
                var client = _httpClientFactory.CreateClient();
                var response = await client.GetAsync($"{AuthorityUri}/.well-known/openid-configuration/jwks");
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(content);
                return jwks?.Keys ?? Enumerable.Empty<SecurityKey>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while fetching signing keys.");
                return Enumerable.Empty<SecurityKey>();
            }
        }
    }
}
