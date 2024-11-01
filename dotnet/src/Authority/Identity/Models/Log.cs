using Agience.Core.Models.Entities;
using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Authority.Models.Manage.Log), ReverseMap = true)]
    public class Log : AgienceEntity
    {
        public string AgentId { get; set; } = null!;

        [ForeignKey(nameof(AgentId))]
        public virtual Agent? Agent { get; set; }
        public string? LogText { get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
