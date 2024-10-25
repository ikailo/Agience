using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(SDK.Models.Entities.Agency), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Agency), ReverseMap = true)]
    public class Agency : Authority.Models.Manage.Agency
    {
        [JsonPropertyName("director_id")]
        public string DirectorId { get; set; } = string.Empty;

        [ForeignKey("DirectorId")]
        [JsonIgnore]
        public virtual Person? Director { get; set; }

        [JsonPropertyName("agents")]
        public virtual new List<Agent> Agents { get; set; } = new List<Agent>();
    }
}
