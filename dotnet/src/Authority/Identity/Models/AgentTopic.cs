using Agience.Core.Models.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class AgentTopic : BaseEntity
    {
        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual Agent? Agent { get; set; }

        [ForeignKey(nameof(TopicId))]
        [JsonIgnore]
        public virtual Topic? Topic { get; set; }


        [JsonPropertyName("agent_id")]
        public string? AgentId { get; set; }

        [JsonPropertyName("topic_id")]
        public string? TopicId { get; set; }
        
    }
}