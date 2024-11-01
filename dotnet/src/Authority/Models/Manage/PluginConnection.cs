using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agience.Authority.Models.Manage
{
    public class PluginConnection : Core.Models.Entities.PluginConnection
    {
        [ForeignKey(nameof(PluginId))]
        [JsonIgnore]
        public virtual Plugin? Plugin { get; set; }

        [JsonPropertyName("plugin_name")]
        public virtual string PluginName => Plugin?.Name ?? string.Empty;
    }
}
