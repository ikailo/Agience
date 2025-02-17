using System.Text.Json.Serialization;

namespace Agience.Core.Interfaces
{
    internal interface IHasOwner
    {
        [JsonPropertyName("owner_id")]
        public string? OwnerId { get; }
    }
}
