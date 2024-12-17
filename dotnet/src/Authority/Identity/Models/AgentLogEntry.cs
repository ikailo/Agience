using Agience.Core.Models.Entities.Abstract;
using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.AgentLogEntry), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.AgentLogEntry), ReverseMap = true)]
    public class AgentLogEntry : Authority.Models.Manage.AgentLogEntry
    {
        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual Agent? Agent { get; set; }
    }
}
