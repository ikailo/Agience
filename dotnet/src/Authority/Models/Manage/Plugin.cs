using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class Plugin : Core.Models.Entities.Plugin
    {
        [JsonPropertyName("connections")]
        public virtual List<PluginConnection> Connections { get; set; } = new List<PluginConnection>();
    }    
}