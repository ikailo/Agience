using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Topic), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Topic), ReverseMap = true)]
    public class Topic : Authority.Models.Manage.Topic
    {
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public virtual new Person? Owner { get; set; }


        [JsonPropertyName("agents")]
        public virtual List<Agent> Agents { get; set; } = new();
    }
}
