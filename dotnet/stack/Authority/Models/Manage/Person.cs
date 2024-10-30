using System.Text.Json.Serialization;
using Agience.SDK.Models.Entities;

namespace Agience.Authority.Models.Manage
{
    public class Person : AgienceEntity
    {
        [JsonPropertyName("provider_id")]
        public string ProviderId { get; set; } = string.Empty;

        [JsonPropertyName("provider_person_id")]
        public string ProviderPersonId { get; set; } = string.Empty;

        [JsonPropertyName("last_login")]
        public DateTime? LastLogin { get; set; }

        [JsonPropertyName("first_name")]
        public string? FirstName { get; set; }

        [JsonPropertyName("last_name")]
        public string? LastName { get; set; }

        [JsonPropertyName("name")]
        public string Name => string.IsNullOrWhiteSpace(FirstName) && string.IsNullOrWhiteSpace(LastName) ? string.Empty : $"{FirstName} {LastName}".Trim();

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        //[JsonPropertyName("terms_accepted_timestamp")] TODO: Require Acceptance, Log DateTime stamp
        //public DateTime? TermsAcceptedTimestamp { get; set; }
    }
}