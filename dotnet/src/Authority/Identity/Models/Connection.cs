using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Connection), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Connection), ReverseMap = true)]
    public class Connection : Authority.Models.Manage.Connection
    {
        [JsonPropertyName("owner")]
        public virtual new Person? Owner { get; set; }

        [JsonPropertyName("functions")]
        public virtual List<Function> Functions { get; set; } = new();

        [JsonPropertyName("authorizers")]
        public virtual List<Authorizer> Authorizers { get; set; } = new();

    }
}
