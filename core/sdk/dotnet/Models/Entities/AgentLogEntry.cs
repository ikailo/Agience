using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class AgentLogEntry : BaseEntity
    {
        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = null!;

        [JsonPropertyName("log_text")]
        public string? LogText { get; set; }

        [JsonPropertyName("created_date")]
        public DateTime? CreatedDate { get; set; }
    }
}