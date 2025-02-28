using Microsoft.Extensions.Logging;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities.Abstract
{
    public abstract class BaseEntity
    {
        [JsonPropertyName("id")]
        public string? Id { get; set; }

        [JsonPropertyName("created_date")]
        public DateTime? CreatedDate { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Dictionary<string, object?> Metadata { get; set; } = new();

    }
}
