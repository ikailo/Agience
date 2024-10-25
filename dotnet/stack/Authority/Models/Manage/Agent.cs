using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Agent : SDK.Models.Entities.Agent
    {
        [JsonPropertyName("connections")]
        public virtual List<AgentConnection> Connections { get; set; } = new List<AgentConnection>();

        [JsonPropertyName("is_connected")]
        [NotMapped]
        public bool IsConnected { get; set; }
    }
}