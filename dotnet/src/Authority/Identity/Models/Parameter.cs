using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Parameter), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Parameter), ReverseMap = true)]
    public class Parameter : Authority.Models.Manage.Parameter
    {


        [ForeignKey("FunctionId")]
        [JsonIgnore]
        public virtual Function? Function { get;  set; }
    }
}
