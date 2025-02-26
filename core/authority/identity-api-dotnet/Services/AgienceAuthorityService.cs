using Duende.IdentityServer.Validation;
using IdentityModel;
using System.Security.Claims;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using Timer = System.Timers.Timer;

namespace Agience.Authority.Identity.Services
{
    public class AgienceAuthorityService : IHostedService
    {
        private readonly Core.Authority _authority;
        private readonly AppConfig _appConfig;
        private readonly IHostApplicationLifetime _appLifetime;
        private readonly ITokenService _tokenService;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<AgienceAuthorityService> _logger;

        private Timer _authorityReconnectTimer = new Timer();

        public AgienceAuthorityService(
            Core.Authority authority,
            AppConfig appConfig,
            IHostApplicationLifetime appLifetime,
            IServiceScopeFactory scopeFactory,
            ITokenService tokenService,
            ILogger<AgienceAuthorityService> logger
            )
        {
            _authority = authority;
            _appConfig = appConfig;
            _appLifetime = appLifetime;
            _tokenService = tokenService;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            if (_appConfig.BypassAuthorityService || Environment.GetEnvironmentVariable("AGIENCE_INITIALIZE")?.ToUpper() == "TRUE")
            {
                _logger.LogInformation("Agience Initialization. Bypassing Authority Service."); 
                return;
            }

            _appLifetime.ApplicationStarted.Register(async () =>
            {
                _logger.LogInformation("Starting Authority");

                await _authority.Connect(await GenerateJwtTokenAsync());

                _logger.LogInformation("Authority Connected");

                // Reconnect occasionally with refreshed JWT token
                // TODO: This leaves short gaps in connectivity and subscriptions. Need to make it more seamless. Probably better to handle in the Broker.
                _authorityReconnectTimer.Elapsed += async (sender, args) =>
                {

                    _logger.LogInformation("Refreshing Authority Connection");
                    await _authority.Disconnect();
                    await _authority.Connect(await GenerateJwtTokenAsync());
                    _logger.LogInformation("Authority Reconnected");
                };
                _authorityReconnectTimer.Interval = TimeSpan.FromHours(23).TotalMilliseconds;
                _authorityReconnectTimer.AutoReset = true;
                _authorityReconnectTimer.Start();

            });

            await Task.CompletedTask;
        }

        internal async Task<string> GenerateJwtTokenAsync()
        {
            if (_authority == null) { throw new ArgumentNullException(nameof(_authority)); }

            var client = new Client
            {
                ClientId = _authority.Id,
                ClientName = _authority.Id,
                AllowedGrantTypes = GrantTypes.ClientCredentials,
                AllowedScopes = new List<string> { "connect" },
                ClientClaimsPrefix = "",
                AccessTokenLifetime = 60 * 60 * 24, // Token lifetime set to 1 day
            };

            var validatedRequest = new ValidatedRequest
            {
                IssuerName = (_appConfig.IssuerUri ?? throw new ArgumentNullException(nameof(_appConfig.IssuerUri))).AbsoluteUri,
                ClientId = client.ClientId,
                Client = client,
                AccessTokenLifetime = client.AccessTokenLifetime,
                ClientClaims = new List<Claim>
                {
                    new Claim("authority_id", _authority.Id),
                    new Claim(JwtClaimTypes.Role, "authority"),
                    new Claim(JwtClaimTypes.ClientId, _authority.Id)
                }
            };

            var tokenCreationRequest = new TokenCreationRequest
            {
                ValidatedRequest = validatedRequest,
                ValidatedResources = new ResourceValidationResult(new Resources
                {
                    ApiScopes = { _appConfig.ApiScopes.First(x => x.Name == "connect") },
                    ApiResources = { _appConfig.ApiResources.First(x => x.Name == "connect-mqtt") },
                    OfflineAccess = false,
                }),
            };

            var token = await _tokenService.CreateAccessTokenAsync(tokenCreationRequest);
            var serializedToken = await _tokenService.CreateSecurityTokenAsync(token);

            _logger.LogInformation($"Generated JWT Token: {token.ClientId} => {serializedToken}");

            return serializedToken;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            if (_authority != null)
            {
                _logger.LogInformation("Disconnecting Broker");

                await _authority.Disconnect();

                _logger.LogInformation("Broker Disconnected");
            }
        }
    }
}
