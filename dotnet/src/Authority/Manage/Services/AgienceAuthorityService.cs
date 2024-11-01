using System.Net.Http.Headers;
using Microsoft.AspNetCore.Authentication;

namespace Agience.Authority.Manage.Services
{
    public class AgienceAuthorityService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IHttpContextAccessor _httpContextAccessor;       

        public string AuthorityUri { get; }

        public AgienceAuthorityService(IHttpClientFactory httpClientFactory, IHttpContextAccessor httpContextAccessor, string authorityUri)
        {
            _httpClientFactory = httpClientFactory;
            _httpContextAccessor = httpContextAccessor;
            AuthorityUri = authorityUri;
        }

        public HttpClient GetHttpClient()
        {
            var httpClient = _httpClientFactory.CreateClient("authority");

            var authenticateResult = _httpContextAccessor.HttpContext?.AuthenticateAsync().Result;

            if (authenticateResult?.Succeeded ?? false)
            {
                var accessToken = authenticateResult.Properties?.GetTokenValue("access_token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            return httpClient;
        }
    }
}