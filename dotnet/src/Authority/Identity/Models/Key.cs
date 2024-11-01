using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Authority.Models.Manage.Key), ReverseMap = true)]
    public class Key : Authority.Models.Manage.Key
    {
        [JsonPropertyName("salted_value")]
        public string? SaltedValue { get; set; }
                
        [ForeignKey("HostId")]
        [JsonIgnore]
        public virtual Host? Host { get; set; }
    }
}