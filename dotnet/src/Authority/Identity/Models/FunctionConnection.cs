using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    //[AutoMap(typeof(Core.Models.Entities.FunctionConnection), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.FunctionConnection), ReverseMap = true)]
    public class FunctionConnection : Authority.Models.Manage.FunctionConnection
    {
        [ForeignKey(nameof(FunctionId))]
        [JsonPropertyName("function")]
        public virtual new Function? Function { get; set; }

        [ForeignKey(nameof(ConnectionId))]
        [JsonPropertyName("connection")]
        public virtual new Connection? Connection { get; set; }     
    }
}