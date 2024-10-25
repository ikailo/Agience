using AutoMapper;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Models
{
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

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonPropertyName("hosts")]
        public virtual List<Host> Hosts { get; set; } = new List<Host>();

        [JsonPropertyName("agencies")]
        public virtual List<Agency> Agencies { get; set; } = new List<Agency>();

        [JsonPropertyName("authorizers")]
        public virtual List<Authorizer> Authorizers { get; set; } = [];
    }
}
