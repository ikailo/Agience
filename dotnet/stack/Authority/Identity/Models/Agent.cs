using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(SDK.Models.Entities.Agent), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Agent), ReverseMap = true)]
    public class Agent : Authority.Models.Manage.Agent
    {
        [ForeignKey(nameof(AgencyId))]
        [JsonIgnore]
        public virtual new Agency? Agency { get; set; }

        [ForeignKey(nameof(HostId))]
        [JsonIgnore]
        public virtual new Host? Host { get; set; }

        [JsonPropertyName("plugins")]
        public virtual new List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonPropertyName("connections")]
        public virtual new List<AgentConnection> Connections { get; set; } = new List<AgentConnection>();


    }
}
