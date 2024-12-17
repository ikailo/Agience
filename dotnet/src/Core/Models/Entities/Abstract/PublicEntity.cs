using Agience.Core.Models.Enums;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities.Abstract
{
    public abstract class PublicEntity : OwnedEntity
    {
        [JsonPropertyName("visibility")]
        public Visibility Visibility { get; set; } = Visibility.Private;

    }
}
