using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {        
        [ConfigurationKeyName("AUTHORITY_ISSUER")] public string? AuthorityIssuer { get; set; }
        [ConfigurationKeyName("AUTHORITY_HOST")] public string? AuthorityHost { get; set; }
        [ConfigurationKeyName("AUTHORITY_PORT")] public string? AuthorityPort { get; set; }
        public string? BrokerUriInternal { get; set; }        
        public string? BrokerUri { get; set; }
        public bool? BypassAuthorityService { get; set; }
        public string? CustomNtpHost { get; set; }
        public string? GoogleClientId { get; set; }
        public string? GoogleClientSecret { get; set; }
        public string? AuthorityDbUri { get; set; }
        public string? AuthorityDbDatabase { get; set; }
        public string? AuthorityDbUsername { get; set; }
        public string? AuthorityDbPassword { get; set; }
        public string? MailchimpApiKey { get; set; }
        public string? MailchimpAudienceId { get; set; }
        public string? MailchimpTags { get; set; }
        public string? WorkspacePath { get; set; }

        public string? FilesRoot { get; set; }

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
            // TODO: Name correctly - should be manage-api and connect-mqtt

            new ApiResource("manage-api")
            {
                Scopes = {"manage"}
            },
            new ApiResource("connect-mqtt")
            {
                Scopes = {"connect"}
            }
        };

        internal IEnumerable<ApiScope> ApiScopes =>
        new List<ApiScope>
        {
            new ApiScope("manage", "Manage API"),
            new ApiScope("connect", "Connect to Messaging Framework")
        };
    }
}