using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Authorizer : PublicEntity
    {        

        [JsonPropertyName("client_id")]
        public string? ClientId { get; set; }

        [JsonPropertyName("client_secret")]
        public string? ClientSecret { get; set; } // TODO: SECURITY: Use a key vault

        [JsonPropertyName("redirect_uri")]
        public string RedirectUriPath => $"/oauth/authorizer/{Id}/callback";

        [JsonPropertyName("auth_uri")]
        public string? AuthUri { get; set; }

        [JsonPropertyName("token_uri")]
        public string? TokenUri { get; set; }

        [JsonPropertyName("scopes")]
        public string? Scopes { get; set; }

        [JsonPropertyName("auth_type")]
        public AuthorizationType AuthType { get; set; } = AuthorizationType.Public;

    }
}

