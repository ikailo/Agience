using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace Agience.Core.Models.Entities
{
    public class Agent : AgienceEntity
    {
        [JsonPropertyName("name")]
        public string? Name { get; set; }

        [JsonPropertyName("description")]
        public string? Description { get; set; }

        [JsonPropertyName("persona")]
        public string? Persona { get; set; }

        [JsonPropertyName("is_enabled")]
        public bool IsEnabled { get; set; } = true;

        [JsonPropertyName("agency_id")]
        public string AgencyId { get; set; } = string.Empty;

        [JsonPropertyName("agency")]
        public virtual Agency? Agency { get; set; }

        [JsonPropertyName("plugins")]
        public virtual List<Plugin> Plugins { get; set; } = new List<Plugin>();

        [JsonPropertyName("chat_completion_function_name")]
        public string? ChatCompletionFunctionName { get; set; }

        [JsonPropertyName("auto_start_function_name")]
        public string? AutoStartFunctionName { get; set; }
        
        [JsonPropertyName("auto_start_function_completion_action")]
        public CompletionAction? AutoStartFunctionCompletionAction { get; set; }
                
        
        // ***************** //
        // Currently, Agents can be associated only with a single Host.
        // TODO: In the future, an Agency will have mupltiple Hosts and an Agent's Functions can be spread accross all of them.
        [JsonPropertyName("host_id")]
        public string? HostId { get; set; }
        [JsonIgnore]
        public virtual Host? Host { get; set; }
        // ***************** //
    }
}