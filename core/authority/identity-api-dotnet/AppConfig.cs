using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {

        [ConfigurationKeyName("AUTHORITY_ISSUER_URI")]
        public Uri? IssuerUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_EXTERNAL_URI")]
        public Uri? ExternalUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_EXTERNAL_PFX_PATH")]
        public string? ExternalCertPath { get; set; }

        [ConfigurationKeyName("BROKER_EXTERNAL_URI")]
        public string? ExternalBrokerUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_INTERNAL_URI")]
        public Uri? InternalUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_INTERNAL_PFX_PATH")]
        public string? InternalCertPath { get; set; }

        [ConfigurationKeyName("BROKER_INTERNAL_URI")]
        public Uri? InternalBrokerUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_BYPASS_AUTHORITY_SERVICE")]
        public bool BypassAuthorityService { get; set; }

        [ConfigurationKeyName("AUTHORITY_DATABASE_HOST")]
        public string? DatabaseHost { get; set; }

        [ConfigurationKeyName("AUTHORITY_DATABASE_PORT")]
        public int DatabasePort { get; set; }

        [ConfigurationKeyName("AUTHORITY_DATABASE_NAME")]
        public string? DatabaseName { get; set; }

        [ConfigurationKeyName("AUTHORITY_DATABASE_USERNAME")]
        public string? DatabaseUsername { get; set; }

        [ConfigurationKeyName("AUTHORITY_DATABASE_PASSWORD")]
        public string? DatabasePassword { get; set; }

        [ConfigurationKeyName("AUTHORITY_GOOGLE_OAUTH_CLIENT_ID")]
        public string? GoogleClientId { get; set; }

        [ConfigurationKeyName("AUTHORITY_GOOGLE_OAUTH_CLIENT_SECRET")]
        public string? GoogleClientSecret { get; set; }

        [ConfigurationKeyName("AUTHORITY_CUSTOM_NTP_HOST")]
        public string? CustomNtpHost { get; set; }

        [ConfigurationKeyName("AUTHORITY_MAILCHIMP_API_KEY")]
        public string? MailchimpApiKey { get; set; }

        [ConfigurationKeyName("AUTHORITY_MAILCHIMP_AUDIENCE_ID")]
        public string? MailchimpAudienceId { get; set; }

        [ConfigurationKeyName("AUTHORITY_MAILCHIMP_TAGS")]
        public string? MailchimpTags { get; set; }


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