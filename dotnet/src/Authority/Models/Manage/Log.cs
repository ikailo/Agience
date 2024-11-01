using Agience.Core.Models.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Agience.Authority.Models.Manage
{
    public class Log : AgienceEntity
    {
        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = null!;

        [JsonPropertyName("log_text")]
        public string? LogText { get; set; }

        [JsonPropertyName("created_date")]
        public DateTime? CreatedDate { get; set; }
    }
}
