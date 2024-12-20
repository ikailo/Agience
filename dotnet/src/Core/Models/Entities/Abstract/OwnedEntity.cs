using System;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities.Abstract
{
    public abstract class OwnedEntity : DescribedEntity
    {
        [JsonPropertyName("owner_id")]
        public string? OwnerId { get; set; }

        [JsonPropertyName("owner")]
        public virtual Person? Owner { get; set; }

    }
}
