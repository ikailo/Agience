using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.SDK.Models.Entities
{
    public class Plugin : AgienceEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("type")]
        public PluginType Type { get; set; } = PluginType.Curated;

        [JsonPropertyName("visibility")]
        public Visibility Visibility { get; set; } = Visibility.Private;

        [NotMapped]
        [JsonPropertyName("functions")]
        public virtual List<Function> Functions { get; set; } = new List<Function>();

    }
}