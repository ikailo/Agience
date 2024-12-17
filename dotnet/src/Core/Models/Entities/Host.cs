using Agience.Core.Models.Entities.Abstract;
using Agience.Core.Models.Enums;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Host : PublicEntity
    {
        [JsonPropertyName("agents")]
        public virtual List<Agent> Agents { get; set; } = new();

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new();

    }
}