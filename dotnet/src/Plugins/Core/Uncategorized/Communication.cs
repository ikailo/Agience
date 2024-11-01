using Microsoft.SemanticKernel;
using System.ComponentModel;
using System.Text;
using System.Text.Json;

namespace Agience.Plugins.Core.Uncategorized
{
    public sealed class Communication
    {
        private readonly HttpClient _httpClient;

        public Communication()
        {
            // Consider using IHttpClientFactory in real applications for better HttpClient management.
            _httpClient = new HttpClient();
        }

        [KernelFunction, Description("Send a prompt to an AI service and receive a response.")]
        [return: Description("The AI service's response.")]
        public async Task<string> SendPrompt(
            [Description("The prompt to send to the AI service.")] string promptContent)
        {
            // Replace these with actual configuration values.
            const string apiKey = "REDACTED";
            const string endpoint = "https://api.openai.com/v1/chat/completions";
            const string model = "gpt-3.5-turbo";

            var request = new
            {
                model,
                messages = new[]
                {
                    new { role = "user", content = promptContent }
                }
            };

            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);
            var content = new StringContent(JsonSerializer.Serialize(request), Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            if (response.IsSuccessStatusCode)
            {
                var result = JsonSerializer.Deserialize<ChatCompletionResponse>(await response.Content.ReadAsStringAsync());
                return result?.Choices?[0]?.Message?.Content ?? "No response received.";
            }
            else
            {
                return $"Error: {response.StatusCode}";
            }
        }

        private class ChatCompletionResponse
        {
            public List<Choice> Choices { get; set; }
        }

        private class Choice
        {
            public Message Message { get; set; }
        }

        private class Message
        {
            public string Content { get; set; }
        }
    }
}
