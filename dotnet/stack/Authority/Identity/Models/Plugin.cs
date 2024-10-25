using AutoMapper;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(SDK.Models.Entities.Plugin), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Plugin), ReverseMap = true)]
    public class Plugin : Authority.Models.Manage.Plugin
    {
        [JsonPropertyName("creator_id")]
        public string CreatorId { get; set; } = string.Empty;

        [ForeignKey("CreatorId")]
        [JsonIgnore]
        public virtual Person? Creator { get; set; }

        [JsonPropertyName("plugin_functions")]
        public virtual ICollection<PluginFunction> PluginFunctions { get; set; } = new List<PluginFunction>();

        [JsonIgnore]
        public override List<SDK.Models.Entities.Function> Functions => PluginFunctions.Select(pf => pf.Function).Where(f => f != null).Cast<SDK.Models.Entities.Function>().ToList()!;

        [JsonIgnore]
        public virtual List<Host> Hosts { get; set; } = new List<Host>();

        [JsonIgnore]
        public virtual List<Agent> Agents { get; set; } = new List<Agent>();

        [JsonPropertyName("connections")]
        public virtual new List<PluginConnection> Connections { get; set; } = new List<PluginConnection>();

        public NpgsqlTsVector DescriptionSearchVector { get; set; } = null!;

    }
}
