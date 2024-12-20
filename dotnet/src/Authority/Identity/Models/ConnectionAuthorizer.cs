using Agience.Core.Models.Entities.Abstract;
using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{   
    public class ConnectionAuthorizer : BaseEntity
    {
        [JsonPropertyName("connection_id")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("authorizer_id")]
        public string? AuthorizerId { get; set; }

        [ForeignKey(nameof(ConnectionId))]
        [JsonPropertyName("connection")]        
        public virtual Connection? Connection { get; set; }

        [ForeignKey(nameof(AuthorizerId))]
        [JsonPropertyName("authorizer")]
        public virtual Authorizer? Authorizer { get; set; }
    }
}

