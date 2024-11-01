using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text.Json;

namespace Agience.Plugins.Core.Uncategorized
{
    public sealed class TemplateSelector
    {
        [KernelFunction, Description("Select the best template to process the input data.")]
        [return: Description("The selected template ID as a JSON document.")]
        public async Task<string> SelectTemplate(
            [Description("The input data based on which the best template should be selected.")] string inputData)
        {
            // Assuming persona, response format, and other details are constants or retrieved from some configuration.
            const string persona = "You are an expert decision maker. You are tasked with choosing the best template for processing the input.";
            const string responseFormat = "{template_id:string}";
            const string defaultTemplateId = "Agience.Agents.Primary.Templates.OpenAI.Prompt";

            var prompt = "From the JSON list of Template Ids and Descriptions below, select the best one to process the input. " +
                         "Use only the Description field for the selection process. " +
                         "Review each item carefully and consider the input data and the context of the current conversation. " +
                         "If no templates are suitable, use the default: '" + defaultTemplateId + "'.";

            // Mocked list of available templates. In a real scenario, this should come from the system's configuration or a dynamic source.
            var availableTemplates = new[] { "Template1", "Template2", "Template3" }; // Placeholder for actual template list

            var templatesDescription = string.Join("\r\n", availableTemplates.Select((t, index) => $"{index + 1}. {t}"));

            // Build the input for the decision-making process.
            var promptInput = new
            {
                Persona = persona,
                ResponseFormat = responseFormat,
                Prompt = prompt,
                Templates = templatesDescription,
                Input = inputData
            };

            // Convert prompt input to JSON string.
            string promptInputJson = JsonSerializer.Serialize(promptInput);

            // Assuming there's a method to dispatch this JSON to an AI service or another internal mechanism for decision-making.
            // For simplicity, let's assume the result is directly the selected template ID.
            string selectedTemplateId = await DispatchDecisionMaking(promptInputJson);

            // Return the selected template ID as a JSON document.
            return JsonSerializer.Serialize(new { template_id = selectedTemplateId });
        }

        // Placeholder for the actual decision-making dispatch logic.
        private async Task<string> DispatchDecisionMaking(string promptInputJson)
        {
            // Mock decision-making process.
            // In a real scenario, this would involve sending the promptInputJson to an AI service or using internal logic to decide.
            await Task.Delay(100); // Simulate async work.

            // Return a mock template ID for demonstration purposes.
            return "MockTemplateId";
        }
    }
}
