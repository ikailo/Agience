using Agience.Authority.Identity.Data.Adapters;
using Host = Agience.Authority.Identity.Models.Host;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Stores;
using IdentityModel;

namespace Agience.Authority.Identity.Data
{
    internal class AgienceHostStore : IClientStore
    {
        private readonly IAgienceDataAdapter _dataAdapter;
        private readonly AppConfig _appConfig;

        public AgienceHostStore(IAgienceDataAdapter dataAdapter, AppConfig config)
        {
            _dataAdapter = dataAdapter;
            _appConfig = config;
        }

        public async Task<Client?> FindClientByIdAsync(string hostId)
        {
            var host = await _dataAdapter.GetRecordByIdAsync<Host>(hostId);            

            if (host == null) { return null; }

            var clientIdentity = new Client()
            {
                ClientName = host.Name,
                ClientId = host.Id ?? throw new Exception("Host id not found"), // TODO: why throw exceptions?
                ClientSecrets = host.Keys?.Select(hostKey => new Secret($"{hostKey.SaltedValue}")).ToList() ?? throw new Exception("Key value not provided."),
                RedirectUris = host.RedirectUris?.Split(' ') ?? new string[0],
                PostLogoutRedirectUris = host.PostLogoutUris?.Split(' ') ?? new string[0],
                AllowedScopes = host.Scopes,
                EnableLocalLogin = false,
                AlwaysIncludeUserClaimsInIdToken = true,
                AccessTokenLifetime = 60 * 60 * 24, // 1 day
                ClientClaimsPrefix = "",
                Claims = new List<ClientClaim> {
                    new ClientClaim(JwtClaimTypes.Role, "host"),
                    new ClientClaim("authority_id", new Uri(_appConfig.AuthorityUri ?? throw new ArgumentNullException("AuthorityUri")).Host),
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