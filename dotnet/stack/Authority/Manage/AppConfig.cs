namespace Agience.Authority.Manage.Web
{
    public class AppConfig : SDK.HostConfig
    {
        public string? CustomNtpHost { get; set; }
        public string? AuthorityUriInternal { get; set; }
        public string? BrokerUriInternal { get; set; }
        public string? HostOpenAiApiKey { get; set; }
    }
}