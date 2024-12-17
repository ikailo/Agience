using Agience.Core.Models.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class AgentPlugin : BaseEntity
    {
        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual Agent Agent { get; set; } = default!;

        [ForeignKey(nameof(PluginId))]
        [JsonIgnore]
        public virtual Plugin Plugin { get; set; } = default!;

        
        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = string.Empty;

        [JsonPropertyName("plugin_id")]
        public string PluginId { get; set; } = string.Empty;
    }
}