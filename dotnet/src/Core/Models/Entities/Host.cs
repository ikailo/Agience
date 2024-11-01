using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Host : AgienceEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("agents")]
        public virtual List<Agent> Agents { get; set; } = new List<Agent>();

        [JsonPropertyName("visibility")]
        public Visibility Visibility { get; set; } = Visibility.Private;
    }
}