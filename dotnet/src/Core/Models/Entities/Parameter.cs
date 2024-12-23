using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Parameter : DescribedEntity
    {
        [JsonPropertyName("type")]
        public string? Type { get; set; }

        [JsonPropertyName("input_function_id")]
        public string? InputFunctionId { get; set; }

        [JsonPropertyName("output_function_id")]
        public string? OutputFunctionId { get; set; }
    }
}