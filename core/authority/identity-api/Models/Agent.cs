using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Agent), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Agent), ReverseMap = true)]
    public class Agent : Authority.Models.Manage.Agent
    {
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public virtual new Person? Owner { get; set; }

        [ForeignKey(nameof(HostId))]
        [JsonIgnore]
        public virtual new Host? Host { get; set; }

        [ForeignKey(nameof(ExecutiveFunctionId))]
        [JsonIgnore]
        public virtual new Function? ExecutiveFunction { get; set; }

        [ForeignKey(nameof(AutoStartFunctionId))]
        [JsonIgnore]
        public virtual new Function? AutoStartFunction { get; set; }

        [JsonPropertyName("plugins")]
        public virtual new List<Plugin> Plugins { get; set; } = new();

        [JsonPropertyName("topics")]
        public virtual new List<Topic> Topics { get; set; } = new();

        [JsonPropertyName("log_entries")]
        public virtual List<AgentLogEntry> LogEntries { get; set; } = new();
    }
}
