using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Connection : PublicEntity
    { 
        [JsonPropertyName("resource_uri")]
        public string? ResourceUri { get; set; }
       
    }
}

