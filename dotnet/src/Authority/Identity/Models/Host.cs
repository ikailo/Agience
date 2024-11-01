using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Host), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Host), ReverseMap = true)]
    public class Host : Authority.Models.Manage.Host
    {
        [JsonPropertyName("operator_id")]
        public string OperatorId { get; set; } = string.Empty;

        [ForeignKey("OperatorId")]
        [JsonIgnore]
        public virtual Person? Operator { get; set; }

        [JsonPropertyName("plugins")]
        public virtual new List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonPropertyName("keys")]
        public virtual new List<Key> Keys { get; set; } = new List<Key>();

        [JsonPropertyName("agents")]
        public virtual new List<Agent> Agents { get; set; } = new List<Agent>();
    }
}