using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {
        public string? AuthorityUriInternal { get; set; }
        public string? BrokerUriInternal { get; set; }
        public string? AuthorityUri { get; set; }
        public string? BrokerUri { get; set; }
        public string? CustomNtpHost { get; set; }
        public string? GoogleClientId { get; set; }
        public string? GoogleClientSecret { get; set; }
        public string? AuthorityDbUri { get; set; }
        public string? AuthorityDbDatabase { get; set; }
        public string? AuthorityDbUsername { get; set; }
        public string? AuthorityDbPassword { get; set; }        

        internal IEnumerable<IdentityResource> IdentityResources =>
            new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email()
            };

        internal IEnumerable<ApiResource> ApiResources =>
            new List<ApiResource>
            {
                new ApiResource("/manage/*")
                {
                    Scopes = {"manage"}
                },
                new ApiResource("/connect/*")
                {
                    Scopes = {"connect"}
                }
            };

        internal IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
            {
                new ApiScope("manage"),
                new ApiScope("connect")
            };
    }
}