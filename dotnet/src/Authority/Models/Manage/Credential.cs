

using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Credential : Core.Models.Entities.Credential
    {
        [NotMapped]
        [JsonPropertyName("has_refresh_token")]
        public bool HasRefreshToken { get; set; }

        [NotMapped]
        [JsonPropertyName("has_access_token")]
        public bool HasAccessToken { get; set; } = false;

        [JsonPropertyName("connection")]
        public virtual Connection? Connection { get; set; }

        [JsonPropertyName("authorizer")]
        public virtual Authorizer? Authorizer { get; set; }

        // ** TRANSIENT ** //

        [JsonIgnore]
        [NotMapped]
        public Plugin? Plugin { get; set; }

        [JsonIgnore]
        [NotMapped]
        public Function? Function { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? PluginId { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string? FunctionId { get; set; } 

    }
}
