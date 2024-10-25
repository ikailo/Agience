using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class PluginFunction
    {
        [JsonPropertyName("plugin_id")]
        public string PluginId { get; set; } = string.Empty;

        [JsonPropertyName("function_id")]
        public string FunctionId { get; set; } = string.Empty;

        [ForeignKey("PluginId")]
        [JsonIgnore]
        public virtual Plugin? Plugin { get; set; }

        [ForeignKey("FunctionId")]
        [JsonIgnore]
        public virtual Function? Function { get; set; }        

        [JsonPropertyName("is_root")]
        public bool IsRoot { get; set; }
    }    
}