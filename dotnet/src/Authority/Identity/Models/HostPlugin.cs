using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class HostPlugin
    {
        [JsonPropertyName("host_id")]
        public string HostId { get; set; } = string.Empty;

        [JsonPropertyName("plugin_id")]
        public string PluginId { get; set; } = string.Empty;

        [ForeignKey(nameof(HostId))]
        [JsonIgnore]
        public virtual Host Host { get; set; } = default!;

        [ForeignKey(nameof(PluginId))]
        [JsonIgnore]
        public virtual Plugin Plugin { get; set; } = default!;
    }
}