using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;

namespace Agience.Authority.Manage.Services
{
    using Microsoft.IdentityModel.Tokens;
    using System.IdentityModel.Tokens.Jwt;
    using System.Security.Claims;

    public class SessionHandlerService
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SessionHandlerService> _logger;
        private readonly AuthorityService _authorityService;
        private readonly IMemoryCache _cache;
        private readonly TokenValidationParameters _tokenValidationParameters;
        private readonly AppConfig _appConfig;

        // Cache expiration duration for session checks
        private static readonly TimeSpan SessionCacheDuration = TimeSpan.FromMinutes(5);

        public SessionHandlerService(
            RequestDelegate next,
            ILogger<SessionHandlerService> logger,
            AuthorityService authorityService,
            IMemoryCache cache,
            AppConfig appConfig
            )
        {
            _next = next;
            _logger = logger;
            _authorityService = authorityService;
            _cache = cache;
            _appConfig = appConfig;

            _tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = _appConfig.AuthorityUri,

                ValidateAudience = true,
                ValidAudiences = new[] { "manage-api", "connect-mqtt" },

                ValidateLifetime = true,
                ClockSkew = TimeSpan.FromMinutes(5),

                ValidateIssuerSigningKey = true,
                IssuerSigningKeys = _authorityService.FetchSigningKeysAsync().Result // Fetch signing keys synchronously
            };
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (ShouldSkipSessionCheck(context.Request.Path))
            {
                await _next(context);
                return;
            }

            try
            {
                var token = await context.GetTokenAsync("access_token");
                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("Access token is missing.");
                    AddInactiveSessionHeader(context);
                }
                else if (!ValidateJwt(token, out ClaimsPrincipal? principal))
                {
                    _logger.LogWarning("Token validation failed.");
                    AddInactiveSessionHeader(context);
                }
                else
                {
                    var userIdentifier = GetUserIdentifier(principal);
                    if (string.IsNullOrEmpty(userIdentifier) || !await IsSessionActiveWithCachingAsync(userIdentifier))
                    {
                        _logger.LogWarning("Session is inactive.");
                        AddInactiveSessionHeader(context);
                    }                    
                }

                await _next(context);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in session handler middleware.");
            }
        }

        private bool ValidateJwt(string token, out ClaimsPrincipal? principal)
        {
            var handler = new JwtSecurityTokenHandler();
            principal = null;

            try
            {
                principal = handler.ValidateToken(token, _tokenValidationParameters, out _);
                return true;
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogWarning($"Token validation failed: {ex.Message}");
                return false;
            }
        }

        private async Task<bool> IsSessionActiveWithCachingAsync(string userIdentifier)
        {
            if (_cache.TryGetValue($"SessionActive:{userIdentifier}", out bool isActive))
            {
                return isActive;
            }

            isActive = await _authorityService.CheckUserInfoEndpointAsync(userIdentifier);
            _cache.Set($"SessionActive:{userIdentifier}", isActive, SessionCacheDuration);
            return isActive;
        }

        private string? GetUserIdentifier(ClaimsPrincipal? user)
        {
            return user?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private void AddInactiveSessionHeader(HttpContext context)
        {
            _logger.LogWarning("Session is inactive. Informing client via header.");
            context.Response.Headers["X-Session-Active"] = "false";
            //context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        }

        private bool ShouldSkipSessionCheck(PathString path)
        {
            var excludedPaths = new[]
            {
                new PathString("/auth"),   // Authentication endpoints
                new PathString("/css"),    // Static CSS files
                new PathString("/js"),     // Static JS files
                new PathString("/images"), // Static image files
                new PathString("/favicon.ico"), // Favicon
                new PathString("/signin-oidc"), // OIDC signin callback
                new PathString("/signout-oidc")  // OIDC signout callback
            };

            if (excludedPaths.Any(path.StartsWithSegments))
            {
                _logger.LogDebug($"Skipping session check for {path}");
                return true;
            }

            _logger.LogDebug($"Checking session for {path}");
            return false;
        }
    }

}
