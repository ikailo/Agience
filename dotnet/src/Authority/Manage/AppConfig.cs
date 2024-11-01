namespace Agience.Authority.Manage
{
    public class AppConfig : Core.HostConfig
    {
        public string? CustomNtpHost { get; set; }
        public string? AuthorityUriInternal { get; set; }
        public string? BrokerUriInternal { get; set; }
        public string? HostOpenAiApiKey { get; set; }
    }
}