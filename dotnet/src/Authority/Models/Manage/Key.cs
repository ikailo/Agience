using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agience.Core.Models.Entities;

namespace Agience.Authority.Models.Manage;

public class Key : AgienceEntity
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("host_id")]
    public string HostId { get; set; } = string.Empty;

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [JsonPropertyName("created_date")]
    public DateTime? CreatedDate { get; set; } = null;

    [NotMapped]
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [NotMapped]
    [JsonPropertyName("is_encrypted")]
    public bool IsEncrypted { get; set; } = false;
}