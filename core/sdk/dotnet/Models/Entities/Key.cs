using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Key : NamedEntity
    {
        [JsonPropertyName("host_id")]
        public string HostId { get; set; } = null!;

        [JsonPropertyName("is_active")]
        public bool IsActive { get; set; } = true;

        [JsonPropertyName("created_date")]
        public DateTime? CreatedDate { get; set; } = null;
    }
}

