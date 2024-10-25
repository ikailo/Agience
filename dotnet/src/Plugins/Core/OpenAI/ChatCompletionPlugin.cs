using System.ComponentModel;
using Agience.SDK.Services;
using Agience.SDK.Attributes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Agience.Plugins.Primary.OpenAI
{
    [PluginConnection(CONNECTION_NAME)]
    public class ChatCompletionPlugin
    {
        private const string CONNECTION_NAME = "OpenAI";
        private const string HOST_CONNECTION_NAME = "HostOpenAiApiKey";

        private readonly string _apiKey;

        public ChatCompletionPlugin(AgienceCredentialService credentialService)
        {   
            _apiKey = credentialService.GetCredential(CONNECTION_NAME) ?? 
                credentialService.GetCredential(HOST_CONNECTION_NAME) ?? 
                throw new ArgumentNullException("OpenAI API Key is missing.");
        }

        [KernelFunction, Description("Get multiple chat content choices for the prompt and settings.")]
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            [Description("The chat history context.")] ChatHistory chatHistory,
            [Description("The AI execution settings.")] OpenAIPromptExecutionSettings? executionSettings = null,
            [Description("The Kernel containing services, plugins, and other state for use throughout the operation.")] Kernel? kernel = null,
            [Description("The CancellationToken to monitor for cancellation requests.")] CancellationToken cancellationToken = default
            )
        {            
            var chatCompletionService = new OpenAIChatCompletionService("gpt-3.5-turbo", _apiKey, null, null, kernel?.LoggerFactory); // TODO: Options
            return await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        }
    }
}