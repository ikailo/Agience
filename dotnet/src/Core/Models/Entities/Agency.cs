using System.Text.Json.Serialization;

namespace Agience.SDK.Models.Entities
{
    public class Agency : AgienceEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }
}

