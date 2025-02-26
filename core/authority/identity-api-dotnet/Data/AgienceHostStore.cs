using Host = Agience.Authority.Identity.Models.Host;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityModel;
using Agience.Authority.Identity.Data.Repositories;

namespace Agience.Authority.Identity.Data
{
    internal class AgienceHostStore : IClientStore
    {
        private readonly RecordsRepository _repository;
        private readonly AppConfig _appConfig;

        public AgienceHostStore(RecordsRepository repository, AppConfig config)
        {
            _repository = repository;
            _appConfig = config;
        }

        public async Task<Client?> FindClientByIdAsync(string hostId)
        {
            var host = await _repository.GetRecordByIdAsSystemAsync<Host>(hostId);

            if (host == null) { return null; }

            var clientIdentity = new Client()
            {
                ClientName = host.Name,
                ClientId = host.Id ?? throw new InvalidOperationException("Host id not found"),
                ClientSecrets = host.Keys?.Select(hostKey => new Secret($"{hostKey.SaltedValue}")).ToList()
                                 ?? throw new InvalidOperationException("Key value not provided."),
                RedirectUris = host.RedirectUris?.Split(' ') ?? new string[0],
                PostLogoutRedirectUris = host.PostLogoutUris?.Split(' ') ?? new string[0],
                AllowedScopes = host.Scopes ?? new(),
                EnableLocalLogin = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = 24 * 60 * 60, // 24 hours
                AbsoluteRefreshTokenLifetime = 30 * 24 * 60 * 60, // 30 days
                SlidingRefreshTokenLifetime = 15 * 24 * 60 * 60, // Sliding expiration of 15 days
                ClientClaimsPrefix = "",
                Claims = new List<ClientClaim> {
                    new ClientClaim(JwtClaimTypes.Role, "host"),
                    new ClientClaim("authority_id", (_appConfig.IssuerUri ?? throw new ArgumentNullException(nameof(_appConfig.IssuerUri))).Host),
                    new ClientClaim("host_id", host.Id)
                }
            };

            if (clientIdentity.AllowedScopes.Contains("connect"))
            {
                clientIdentity.AllowedGrantTypes.Add(GrantType.ClientCredentials);
            }

            if (clientIdentity.AllowedScopes.Contains("manage"))
            {
                clientIdentity.AllowedGrantTypes.Add(GrantType.AuthorizationCode);
            }

            return clientIdentity;
        }
    }
}