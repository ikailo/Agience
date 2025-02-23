using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {        
        [ConfigurationKeyName("AUTHORITY_ISSUER_URI")] public string? AuthorityUri { get; set; }
        [ConfigurationKeyName("AUTHORITY_PORT")] public string? AuthorityPort { get; set; }       
        [ConfigurationKeyName("AUTHORITY_HOST")] public string? AuthorityHost { get; set; }
        public string? AuthorityUriInternal => $"https://{AuthorityHost}:{AuthorityPort}";
        [ConfigurationKeyName("AUTHORITY_BROKER_URI")] public string? BrokerUri { get; set; }
        public string? BrokerUriInternal { get; set; }                
        [ConfigurationKeyName("AUTHORITY_BYPASS_SERVICE")] public bool? BypassAuthorityService { get; set; }
        public string? CustomNtpHost { get; set; }
        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_ID")] public string? GoogleClientId { get; set; }
        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_SECRET")] public string? GoogleClientSecret { get; set; }
        [ConfigurationKeyName("AUTHORITY_DATABASE_HOST")] public string? AuthorityDbHost { get; set; }
        [ConfigurationKeyName("AUTHORITY_DATABASE_PORT")] public string? AuthorityDbPort { get; set; }
        [ConfigurationKeyName("AUTHORITY_DATABASE_NAME")] public string? AuthorityDbDatabaseName { get; set; }
        [ConfigurationKeyName("AUTHORITY_DATABASE_USERNAME")] public string? AuthorityDbUsername { get; set; }
        [ConfigurationKeyName("AUTHORITY_DATABASE_PASSWORD")] public string? AuthorityDbPassword { get; set; }
        public string? MailchimpApiKey { get; set; }
        public string? MailchimpAudienceId { get; set; }
        public string? MailchimpTags { get; set; }
        public string? WorkspacePath { get; set; }

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