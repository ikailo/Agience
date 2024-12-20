using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Key), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Key), ReverseMap = true)]
    public class Key : Authority.Models.Manage.Key
    {
        [ForeignKey(nameof(HostId))]
        [JsonIgnore]
        public virtual Host? Host { get; set; }


        [JsonPropertyName("salted_value")]
        public string? SaltedValue { get; set; }
    }
}