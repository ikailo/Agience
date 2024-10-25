using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{   
    public class AgentConnection : SDK.Models.Entities.AgienceEntity
    {
        [JsonPropertyName("agent_id")]
        public string AgentId { get; set; } = string.Empty;

        [JsonPropertyName("plugin_connection_id")]
        public string PluginConnectionId { get; set; } = string.Empty;

        [JsonPropertyName("credential_id")]
        public string? CredentialId { get; set; }

        [JsonPropertyName("authorizer_id")]
        public string? AuthorizerId { get; set; }

        [JsonPropertyName("status")]
        public ConnectionStatus Status { get; set; } = ConnectionStatus.Inactive;

        [ForeignKey(nameof(AgentId))]
        [JsonIgnore]
        public virtual Agent Agent { get; set; } = default!;

        [ForeignKey(nameof(PluginConnectionId))]
        [JsonPropertyName("plugin_connection")]
        public virtual PluginConnection PluginConnection { get; set; } = default!;

        [ForeignKey(nameof(CredentialId))]
        [JsonIgnore]
        public virtual Credential? Credential { get; set; }

        [ForeignKey(nameof(AuthorizerId))]
        [JsonPropertyName("authorizer")]
        public virtual Authorizer? Authorizer { get; set; }
    }

    public enum ConnectionStatus
    {
        Active,
        Inactive
    }
}
