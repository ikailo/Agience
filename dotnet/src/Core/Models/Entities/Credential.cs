using System.Text.Json.Serialization;
using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;

namespace Agience.Core.Models.Entities
{
    public class Credential : BaseEntity
    {

        [JsonPropertyName("agent_id")]
        public string? AgentId { get; set; }

        [JsonPropertyName("connection_id")]
        public string? ConnectionId { get; set; }

        [JsonPropertyName("authorizer_id")]
        public string? AuthorizerId { get; set; }

        [JsonPropertyName("status")]
        public CredentialStatus Status { get; set; } = CredentialStatus.NoAuthorizer;

        [JsonPropertyName("refresh_token")]
        public string? RefreshToken { get; set; }

        [JsonPropertyName("access_token")]
        public string? AccessToken { get; set; }

        [JsonPropertyName("expiry_date")]
        public DateTime? ExpiryDate { get; set; }

    }
}

