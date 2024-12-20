using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using Agience.Core.Models.Entities;
using Agience.Core.Models.Entities.Abstract;

namespace Agience.Authority.Models.Manage;

public class Key : NamedEntity
{

    [JsonPropertyName("host_id")]
    public string? HostId { get; set; }

    [JsonPropertyName("is_active")]
    public bool IsActive { get; set; } = true;

    [NotMapped]
    [JsonPropertyName("value")]
    public string? Value { get; set; }

    [NotMapped]
    [JsonPropertyName("is_encrypted")]
    public bool IsEncrypted { get; set; } = false;
}