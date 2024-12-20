using Agience.Core.Models.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class HostPlugin : BaseEntity
    {
        [ForeignKey(nameof(HostId))]
        [JsonIgnore]
        public virtual Host? Host { get; set; }

        [ForeignKey(nameof(PluginId))]
        [JsonIgnore]
        public virtual Plugin? Plugin { get; set; }


        [JsonPropertyName("host_id")]
        public string? HostId { get; set; }

        [JsonPropertyName("plugin_id")]
        public string? PluginId { get; set; }
    }
}