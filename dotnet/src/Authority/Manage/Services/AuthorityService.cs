using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication;
using Microsoft.IdentityModel.Tokens;

namespace Agience.Authority.Manage.Services
{
    public class AuthorityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ILogger<AuthorityService> _logger;
        private readonly AppConfig _appConfig;

        public string AuthorityUri => (_appConfig.AuthorityUriInternal ?? _appConfig.AuthorityUri) ?? throw new ArgumentNullException(nameof(_appConfig.AuthorityUri));

        public AuthorityService(
            IHttpClientFactory httpClientFactory,
            IHttpContextAccessor httpContextAccessor,
            ILogger<AuthorityService> logger,
            AppConfig appConfig
            )
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
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

                if (_httpContextAccessor?.HttpContext == null)
                {
                    _logger.LogWarning("HttpContext is null. Unable to authenticate.");
                    return null;
                }

                var authenticateResult = await _httpContextAccessor.HttpContext.AuthenticateAsync();
                if (authenticateResult?.Succeeded ?? false)
                {
                    var accessToken = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                    if (!string.IsNullOrEmpty(accessToken))
                    {
                        _logger.LogDebug("Successfully retrieved access token.");
                        httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                        return httpClient;
                    }
                    else
                    {
                        _logger.LogWarning("Access token is missing.");
                    }
                }
                else
                {
                    _logger.LogWarning("Authentication failed.");
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create and configure HttpClient.");
                return null;
            }
        }

        public async Task<bool> CheckUserInfoEndpointAsync(string userIdentifier)
        {
            try
            {
                var token = await _httpContextAccessor.HttpContext.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Access token is missing.");
                    return false;
                }

                var client = _httpClientFactory.CreateClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var response = await client.GetAsync($"{_appConfig.AuthorityUriInternal ?? _appConfig.AuthorityUri}/connect/userinfo");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while calling /userinfo endpoint.");
                return false;
            }
        }

        public async Task<IEnumerable<SecurityKey>> FetchSigningKeysAsync()
        {
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync($"{_appConfig.AuthorityUriInternal ?? _appConfig.AuthorityUri}/.well-known/openid-configuration/jwks");
            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync();
            var jwks = JsonSerializer.Deserialize<JsonWebKeySet>(content);
            return jwks?.Keys ?? Enumerable.Empty<SecurityKey>();
        }

    }


}