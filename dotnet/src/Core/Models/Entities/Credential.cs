using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Credential : AgienceEntity
    {
        [JsonPropertyName("secret")]
        public string? Secret { get; set; } // TODO: SECURITY: Use a key vault
        
    }
}

