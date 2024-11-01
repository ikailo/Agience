using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Function), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Function), ReverseMap = true)]
    public class Function : Authority.Models.Manage.Function
    {
        [JsonPropertyName("plugin_functions")]
        public virtual ICollection<PluginFunction> PluginFunctions { get; set; } = new List<PluginFunction>();
    }
}
