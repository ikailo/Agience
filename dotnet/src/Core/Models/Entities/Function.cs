using Microsoft.SemanticKernel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Function : AgienceEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("prompt")]
        public string? Prompt { get; set; }        

        //[JsonPropertyName("input_variables")]
        //public List<InputVariable> InputVariables { get; set; } = [];

        //[JsonPropertyName("output_variable")]
        //public OutputVariable? OutputVariable { get; set; }

        //[JsonPropertyName("execution_settings")]
        //public Dictionary<string, PromptExecutionSettings> ExecutionSettings { get; set; } = [];
    }
}
