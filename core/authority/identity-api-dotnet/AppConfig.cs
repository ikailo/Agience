using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {

        [ConfigurationKeyName("AUTHORITY_ISSUER_URI")]
        public string? IssuerUri { get; set; }

        [ConfigurationKeyName("AUTHORITY_SECONDARY_HOST")]
        public string? ExternalHost { get; set; }

        [ConfigurationKeyName("AUTHORITY_SECONDARY_PORT")]
        public int? ExternalPort { get; set; }

        public Uri? ExternalUri => string.IsNullOrEmpty(ExternalHost) ? null : new Uri($"https://{ExternalHost}:{ExternalPort}");

        [ConfigurationKeyName("AUTHORITY_SECONDARY_PFX_PATH")]
        public string? ExternalCertPath { get; set; }       

        [ConfigurationKeyName("AUTHORITY_PRIMARY_HOST")]
        public string? InternalHost { get; set; }

        [ConfigurationKeyName("AUTHORITY_PRIMARY_PORT")]
        public int InternalPort { get; set; }

        public Uri InternalUri => new Uri($"https://{InternalHost}:{InternalPort}");

        [ConfigurationKeyName("AUTHORITY_PRIMARY_PFX_PATH")]
        public string? InternalCertPath { get; set; }

        [ConfigurationKeyName("BROKER_PUBLIC_URI")]
        public Uri? ExternalBrokerUri { get; set; }

        [ConfigurationKeyName("BROKER_HOST")]
        public string? InternalBrokerHost { get; set; }

        [ConfigurationKeyName("BROKER_PORT")]
        public int InternalBrokerPort { get; set; }       
        
        public Uri InternalBrokerUri => new Uri($"https://{InternalBrokerHost}:{InternalBrokerPort}");

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

        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_ID")]
        public string? GoogleClientId { get; set; }

        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_SECRET")]
        public string? GoogleClientSecret { get; set; }

        [ConfigurationKeyName("CUSTOM_NTP_HOST")]
        public string? CustomNtpHost { get; set; }

        [ConfigurationKeyName("MAILCHIMP_API_KEY")]
        public string? MailchimpApiKey { get; set; }

        [ConfigurationKeyName("MAILCHIMP_AUDIENCE_ID")]
        public string? MailchimpAudienceId { get; set; }

        [ConfigurationKeyName("MAILCHIMP_TAGS")]
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