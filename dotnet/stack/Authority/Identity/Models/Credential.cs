using AutoMapper;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(SDK.Models.Entities.Credential), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Credential), ReverseMap = true)]
    public class Credential : Authority.Models.Manage.Credential
    {

    }
}
