using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Authority.Models.Manage
{
    public class FunctionConnection : BaseEntity
    {        
        [JsonPropertyName("function")]
        public virtual Function? Function { get; set; }

        [JsonPropertyName("connection")]
        public virtual Connection? Connection { get; set; }

        [JsonPropertyName("function_id")]
        public string? FunctionId { get; set; }

        [JsonPropertyName("connection_id")]
        public string? ConnectionId { get; set; }        
    }
}