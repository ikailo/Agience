using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Host : Core.Models.Entities.Host
    {
        [JsonPropertyName("redirect_uris")]
        public string? RedirectUris { get; set; }
        
        [JsonPropertyName("post_logout_uris")]
        public string? PostLogoutUris { get; set; }

        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; } = new();

        [JsonPropertyName("plugins")]
        public virtual new List<Plugin> Plugins { get; set; } = new();

        [JsonPropertyName("keys")]
        public virtual List<Key> Keys { get; set; } = new();        
    }
}