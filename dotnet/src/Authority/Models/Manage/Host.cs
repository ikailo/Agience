using Agience.Core.Models.Entities;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Host : Core.Models.Entities.Host
    {
        private List<string>? _scopes;

        [JsonPropertyName("redirect_uris")]
        public string? RedirectUris { get; set; }
        
        [JsonPropertyName("post_logout_uris")]
        public string? PostLogoutUris { get; set; }
        
        [JsonPropertyName("scopes")]
        public List<string>? Scopes
        {
            get => _scopes ?? new List<string>();
            set => _scopes = value;
        }

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonPropertyName("keys")]
        public virtual List<Key> Keys { get; set; } = new List<Key>();        
    }
}