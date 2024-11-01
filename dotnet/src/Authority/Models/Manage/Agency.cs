using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agience.Authority.Models.Manage
{
    public class Agency : Core.Models.Entities.Agency
    {
        [JsonPropertyName("agents")]
        public virtual List<Agent> Agents { get; set; } = new List<Agent>();


        [JsonPropertyName("is_connected")]
        [NotMapped]
        public bool IsConnected { get; set; }
    }
}