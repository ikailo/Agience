using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(SDK.Models.Entities.PluginConnection), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.PluginConnection), ReverseMap = true)]
    public class PluginConnection : Authority.Models.Manage.PluginConnection
    {
        [ForeignKey(nameof(PluginId))]
        [JsonPropertyName("plugin")]
        public virtual new Plugin Plugin { get; set; } = default!;

        [JsonPropertyName("plugin_name")]
        public override string PluginName => Plugin?.Name ?? string.Empty;
    }
}
