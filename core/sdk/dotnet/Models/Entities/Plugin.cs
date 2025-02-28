using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Plugin : PublicEntity
    {
        [JsonIgnore]
        [NotMapped]
        public  virtual Type? Type { get; set; }

        [JsonPropertyName("unique_name")]
        public string? UniqueName { get; set; }

        [JsonPropertyName("plugin_provider")]
        public PluginProvider PluginProvider { get; set; } = PluginProvider.Prompt;

        [JsonPropertyName("plugin_source")]
        public PluginSource PluginSource { get; set; } = PluginSource.UserDefined;

        [JsonPropertyName("functions")]
        public virtual List<Function> Functions { get; set; } = new();

    }
}