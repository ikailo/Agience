using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Agent : Core.Models.Entities.Agent
    {
        [JsonPropertyName("executive_function")]
        public virtual Function? ExecutiveFunction { get; set; }

        [JsonPropertyName("auto_start_function")]
        public virtual Function? AutoStartFunction { get; set; }

        [JsonPropertyName("host")]
        public virtual Host? Host { get; set; }
 
        [JsonPropertyName("is_connected")]
        [NotMapped]
        public bool IsConnected { get; set; }
    }
}