using AutoMapper;
using NpgsqlTypes;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Plugin), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Plugin), ReverseMap = true)]
    public class Plugin : Authority.Models.Manage.Plugin
    {
        [ForeignKey(nameof(OwnerId))]
        [JsonIgnore]
        public virtual new Person? Owner { get; set; }

        [JsonPropertyName("functions")]
        public virtual new List<Function> Functions { get; set; } = new();

        //[ForeignKey(nameof(PluginLibraryId))]
        //[JsonIgnore]
        //public virtual PluginLibrary PluginLibrary { get; set; } = new();


    }
}
