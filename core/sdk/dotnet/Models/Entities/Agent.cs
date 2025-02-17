using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Agent : OwnedEntity
    {
        //[JsonPropertyName("host")]
        //public virtual Host? Host { get; set; }

        [JsonPropertyName("topics")]
        public virtual List<Topic> Topics { get; set; } = new();

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new();

        [JsonPropertyName("persona")]
        public string? Persona { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [JsonPropertyName("executive_function_id")]
        public string? ExecutiveFunctionId { get; set; }

        [JsonPropertyName("auto_start_function_id")]
        public string? AutoStartFunctionId { get; set; }
        
        [JsonPropertyName("on_auto_start_function_complete")]
        public CompletionAction? OnAutoStartFunctionComplete { get; set; }               

        [JsonPropertyName("host_id")]
        public string? HostId { get; set; }
    }
}