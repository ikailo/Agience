using Agience.Core.Models.Entities.Abstract;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Topic : PublicEntity
    {
        public string? Address { get; set; }       
    }
}

