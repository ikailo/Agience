using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{   
    [AutoMap(typeof(Authority.Models.Manage.AgentConnection), ReverseMap = true)]
    public class AgentConnection : Authority.Models.Manage.AgentConnection
    {
        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual new Agent Agent { get; set; } = default!;

        [ForeignKey(nameof(PluginConnectionId))]
        [JsonIgnore]
        public virtual new PluginConnection PluginConnection { get; set; } = default!;

        [ForeignKey(nameof(CredentialId))]
        [JsonIgnore]
        public virtual new Credential Credential { get; set; } = default!;

        [ForeignKey(nameof(AuthorizerId))]
        [JsonIgnore]
        public virtual new Authorizer Authorizer { get; set; } = default!;
    }
}
