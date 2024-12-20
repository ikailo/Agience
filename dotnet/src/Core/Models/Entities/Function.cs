using Agience.Core.Models.Entities.Abstract;
using System.Reflection.Metadata;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Function : DescribedEntity
    {
        //[JsonPropertyName("connection_id")]
        //public string? ConnectionId { get; set; }

        [JsonPropertyName("instruction")]
        public string? Instruction { get; set; }

        [JsonPropertyName("inputs")]
        public virtual List<Parameter> Inputs { get; set; } = new();
        
        [JsonPropertyName("outputs")]
        public virtual List<Parameter> Outputs { get; set; } = new();

        //[JsonPropertyName("execution_settings")]
        //public Dictionary<string, PromptExecutionSettings> ExecutionSettings { get; set; } = [];
    }
}
