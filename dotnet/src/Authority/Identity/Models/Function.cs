using AutoMapper;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Function), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Function), ReverseMap = true)]
    public class Function : Authority.Models.Manage.Function
    {
        [JsonPropertyName("connections")]
        public virtual List<Connection> Connections { get; set; } = new();

        [JsonPropertyName("inputs")]
        public virtual new List<Parameter> Inputs { get; set; } = new();

        [JsonPropertyName("outputs")]
        public virtual new List<Parameter> Outputs { get; set; } = new();
    }
}
