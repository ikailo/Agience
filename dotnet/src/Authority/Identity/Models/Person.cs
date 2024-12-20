using AutoMapper;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
    [AutoMap(typeof(Core.Models.Entities.Person), ReverseMap = true)]
    [AutoMap(typeof(Authority.Models.Manage.Person), ReverseMap = true)]
    public class Person : Authority.Models.Manage.Person
    {
        internal const char SEPARATOR = '\u200B';

        internal static (string, string)? ExtractProviderAndId(string providerAndId)
        {
            if (!string.IsNullOrEmpty(providerAndId))
            {
                var parts = providerAndId.Split(SEPARATOR);

                if (parts.Length == 2)
                {
                    return (parts[0], parts[1]);
                }
            }

            return null;
        }

        internal static string CombineProviderAndId(string provider, string id)
        {
            if (string.IsNullOrEmpty(provider) || string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("Provider and Id cannot be null or empty.");
            }

            return $"{provider}{SEPARATOR}{id}";
        }

        [JsonPropertyName("agents")]
        public virtual List<Agent> Agents { get; set; } = new();

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new();

        //[JsonPropertyName("plugins_libraries")]
        //public virtual List<PluginLibrary> PluginLibraries { get; set; } = new();

        [JsonPropertyName("hosts")]
        public virtual List<Host> Hosts { get; set; } = new();

        [JsonPropertyName("topics")]
        public virtual List<Topic> Topics { get; set; } = new();

        [JsonPropertyName("authorizers")]
        public virtual List<Authorizer> Authorizers { get; set; } = new();
        
        [JsonPropertyName("connection")]
        public virtual List<Connection> Connections { get; set; } = new();
    }
}
