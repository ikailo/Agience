using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class AgienceEntity
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty;
    }
}
