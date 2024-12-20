using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Authorizer), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Authorizer), ReverseMap = true)]
    public class Authorizer : Authority.Models.Manage.Authorizer
    {
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public virtual new Person? Owner { get; set; }

        [JsonPropertyName("connections")]
        public virtual List<Connection> Connections { get; set; } = new();
    }
}
