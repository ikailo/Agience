using Agience.Core.Models.Enums;
using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Credential), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Credential), ReverseMap = true)]
    public class Credential : Authority.Models.Manage.Credential
    {
        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual Agent? Agent { get; set; }

        [ForeignKey(nameof(ConnectionId))]
        [JsonIgnore]
        public virtual new Connection? Connection { get; set; }

        [ForeignKey(nameof(AuthorizerId))]
        [JsonIgnore]
        public virtual new Authorizer? Authorizer { get; set; }


    }
}
