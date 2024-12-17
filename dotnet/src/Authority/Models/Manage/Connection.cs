using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations.Schema;

namespace Agience.Authority.Models.Manage
{
    public class Connection : Core.Models.Entities.Connection
    {
        [JsonPropertyName("scopes")]
        public List<string> Scopes { get; set; } = new();
    }
}
