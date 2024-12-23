using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities.Abstract
{
    public abstract class NamedEntity : BaseEntity
    {
        [JsonPropertyName("name")]
        public virtual string Name { get; set; } = string.Empty;

    }
}
