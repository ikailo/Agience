using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Host), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Host), ReverseMap = true)]
    public class Host : Authority.Models.Manage.Host
    {
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public virtual new Person? Owner { get; set; }

        [JsonPropertyName("plugins")]
        public virtual new List<Plugin> Plugins { get; set; } = new();

        [JsonPropertyName("keys")]
        public virtual new List<Key> Keys { get; set; } = new();

        [JsonPropertyName("agents")]
        public virtual new List<Agent> Agents { get; set; } = new();
    }
}