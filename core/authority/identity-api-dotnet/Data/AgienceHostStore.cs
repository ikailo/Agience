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

            if (!string.IsNullOrEmpty(_appConfig.ManageClientId) && hostId == _appConfig.ManageClientId)
            {
                return new Client
                {
                    ClientId = _appConfig.ManageClientId ?? throw new ArgumentNullException(nameof(_appConfig.ManageClientId)),
                    //ClientSecrets = {new Secret(_appConfig.ManageClientSecret ?? throw new ArgumentNullException(nameof(_appConfig.ManageClientSecret)))},
                    ClientName = "Manage UI",
                    AllowedGrantTypes = GrantTypes.Code, // PKCE Support
                    RequirePkce = true,
                    RequireClientSecret = false, // SPA does not require client secret
                    RedirectUris = { _appConfig.ManageRedirectUri ?? throw new ArgumentNullException(nameof(_appConfig.ManageRedirectUri)) },
                    PostLogoutRedirectUris = { _appConfig.ManageLogoutRedirectUri ?? throw new ArgumentNullException(nameof(_appConfig.ManageLogoutRedirectUri)) },
                    AllowedCorsOrigins = { _appConfig.ManageOriginUri ?? throw new ArgumentNullException(nameof(_appConfig.ManageOriginUri)) },
                    AllowedScopes = { "openid", "profile", "email", "manage" },
                    AllowAccessTokensViaBrowser = true, // Allow token access in frontend
                    AccessTokenLifetime = 3600, // Token valid for 1 hour
                    AlwaysIncludeUserClaimsInIdToken = true,
                    EnableLocalLogin = false,
                    Claims = new List<ClientClaim> {
                    new ClientClaim(JwtClaimTypes.Role, "host"),
                    new ClientClaim("authority_id", _appConfig.AuthorityPublicUri ?? throw new ArgumentNullException(nameof(_appConfig.AuthorityPublicUri))),
                    new ClientClaim("host_id", _appConfig.ManageClientId)
                }
                    
                };
            }

            else
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
                    new ClientClaim("authority_id", _appConfig.AuthorityPublicUri ?? throw new ArgumentNullException(nameof(_appConfig.AuthorityPublicUri))),
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
}