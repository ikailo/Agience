using Duende.IdentityServer.Models;

namespace Agience.Authority.Identity
{
    public class AppConfig
    {

        // Public URIs
        [ConfigurationKeyName("AUTHORITY_PUBLIC_URI")]
        public string? AuthorityPublicUri { get; set; }

        [ConfigurationKeyName("BROKER_PUBLIC_URI")]
        public Uri? BrokerPublicUri { get; set; }

        // LAN (Internal) Settings
        [ConfigurationKeyName("LAN_AUTHORITY_HOST")]
        public string? LanAuthorityHost { get; set; }

        [ConfigurationKeyName("LAN_AUTHORITY_PORT")]
        public int LanAuthorityPort { get; set; }

        [ConfigurationKeyName("LAN_BROKER_HOST")]
        public string? LanBrokerHost { get; set; }

        [ConfigurationKeyName("LAN_BROKER_PORT")]
        public int LanBrokerPort { get; set; }

        [ConfigurationKeyName("LAN_DATABASE_HOST")]
        public string? LanDatabaseHost { get; set; }

        [ConfigurationKeyName("LAN_DATABASE_PORT")]
        public int LanDatabasePort { get; set; }

        [ConfigurationKeyName("LAN_MANAGE_HOST")]
        public string? LanManageHost { get; set; }

        // LAN Certificate Paths
        [ConfigurationKeyName("LAN_PFX_PATH")]
        public string? LanPfxPath { get; set; }

        [ConfigurationKeyName("LAN_CRT_PATH")]
        public string? LanCrtPath { get; set; }

        [ConfigurationKeyName("LAN_KEY_PATH")]
        public string? LanKeyPath { get; set; }

        // LAN External Settings
        [ConfigurationKeyName("LAN_EXTERNAL_MANAGE")]
        public bool LanExternalManage { get; set; }

        [ConfigurationKeyName("LAN_EXTERNAL_AUTHORITY")]
        public bool LanExternalAuthority { get; set; }

        [ConfigurationKeyName("LAN_EXTERNAL_HOST")]
        public string? LanExternalHost { get; set; }

        [ConfigurationKeyName("LAN_EXTERNAL_PFX_PATH")]
        public string? LanExternalPfxPath { get; set; }

        [ConfigurationKeyName("LAN_EXTERNAL_CRT_PATH")]
        public string? LanExternalCrtPath { get; set; }

        [ConfigurationKeyName("LAN_EXTERNAL_KEY_PATH")]
        public string? LanExternalKeyPath { get; set; }

        // WAN (External) Settings
        [ConfigurationKeyName("WAN_ENABLED")]
        public bool WanEnabled { get; set; }

        [ConfigurationKeyName("WAN_HOST")]
        public string? WanHost { get; set; }

        [ConfigurationKeyName("WAN_AUTHORITY_PORT")]
        public int WanAuthorityPort { get; set; }

        public Uri? WanAuthorityUri =>
            !string.IsNullOrWhiteSpace(WanHost)
                ? new Uri($"https://{WanHost}:{WanAuthorityPort}")
                : null;

        [ConfigurationKeyName("WAN_BROKER_PORT")]
        public int WanBrokerPort { get; set; }

        public Uri? WanBrokerUri =>
            !string.IsNullOrWhiteSpace(WanHost)
                ? new Uri($"https://{WanHost}:{WanBrokerPort}")
                : null;
        
        [ConfigurationKeyName("MANAGE_UI_CLIENT_ID")]
        public string? ManageClientId { get; set; }

        [ConfigurationKeyName("MANAGE_UI_CLIENT_SECRET")]
        public string? ManageClientSecret { get; set; }

        [ConfigurationKeyName("MANAGE_UI_REDIRECT_URI")]
        public string? ManageRedirectUri { get; set; }

        [ConfigurationKeyName("MANAGE_UI_LOGOUT_REDIRECT_URI")]
        public string? ManageLogoutRedirectUri { get; set; }

        [ConfigurationKeyName("MANAGE_UI_ORIGIN_URI")]
        public string? ManageOriginUri { get; set; }

        // WAN Certificate Paths
        [ConfigurationKeyName("WAN_PFX_PATH")]
        public string? WanPfxPath { get; set; }

        [ConfigurationKeyName("WAN_CRT_PATH")]
        public string? WanCrtPath { get; set; }

        [ConfigurationKeyName("WAN_KEY_PATH")]
        public string? WanKeyPath { get; set; }

        // Database Settings
        public string? DatabaseHost =>
            LanExternalAuthority
                ? WanHost
                : LanDatabaseHost;

        [ConfigurationKeyName("DATABASE_NAME")]
        public string? DatabaseName { get; set; }

        [ConfigurationKeyName("DATABASE_USERNAME")]
        public string? DatabaseUsername { get; set; }

        [ConfigurationKeyName("DATABASE_PASSWORD")]
        public string? DatabasePassword { get; set; }

        // Google OAuth Settings
        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_ID")]
        public string? GoogleOAuthClientId { get; set; }

        [ConfigurationKeyName("GOOGLE_OAUTH_CLIENT_SECRET")]
        public string? GoogleOAuthClientSecret { get; set; }

        // Optional NTP Server (if defined)
        [ConfigurationKeyName("NTP_HOST")]
        public string? CustomNtpHost { get; set; }

        // Mailchimp API Settings (if defined)
        [ConfigurationKeyName("MAILCHIMP_API_KEY")]
        public string? MailchimpApiKey { get; set; }

        [ConfigurationKeyName("MAILCHIMP_AUDIENCE_ID")]
        public string? MailchimpAudienceId { get; set; }

        [ConfigurationKeyName("MAILCHIMP_TAGS")]
        public string? MailchimpTags { get; set; }

        [ConfigurationKeyName("BYPASS_AUTHORITY_SERVICE")]
        public bool BypassAuthorityService { get; set; }

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