using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities.Abstract
{
    public abstract class DescribedEntity : NamedEntity
    {
        [JsonPropertyName("description")]
        public virtual string Description { get; set; } = string.Empty;

    }
}
