using AutoMapper;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Authorizer), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Authorizer), ReverseMap = true)]
    public class Authorizer : Authority.Models.Manage.Authorizer
    {
        [ForeignKey(nameof(ManagerId))]
        [JsonIgnore]
        public virtual Person? Manager { get; set; }
    }
}
