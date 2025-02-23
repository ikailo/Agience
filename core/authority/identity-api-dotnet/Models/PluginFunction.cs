using Agience.Core.Models.Entities.Abstract;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    public class PluginFunction : BaseEntity
    {
        [ForeignKey(nameof(PluginId))]
        [JsonIgnore]
        public virtual Plugin? Plugin { get; set; }

        [ForeignKey(nameof(FunctionId))]
        [JsonIgnore]
        public virtual Function? Function { get; set; }


        [JsonPropertyName("plugin_id")]
        public string? PluginId { get; set; }

        [JsonPropertyName("function_id")]
        public string? FunctionId { get; set; }

        [JsonPropertyName("is_root")]
        public bool IsRoot { get; set; } = false;
    }    
}