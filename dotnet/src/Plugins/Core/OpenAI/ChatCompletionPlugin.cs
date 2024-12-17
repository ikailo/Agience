using System.ComponentModel;
using Agience.Core.Services;
using Agience.Core.Attributes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using Agience.Core.Extensions;

namespace Agience.Plugins.Core.OpenAI
{
    public class ChatCompletionPlugin
    {
        private readonly AgienceCredentialService _credentialService;
        private Kernel _kernel;

        public ChatCompletionPlugin(AgienceCredentialService credentialService, Kernel kernel)
        {
            _credentialService = credentialService;
            _kernel = kernel;
        }

        [AgienceConnection("OpenAI")]
        [KernelFunction, Description("Get multiple chat content choices for the prompt and settings.")]
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            [Description("The chat history context.")] ChatHistory chatHistory,
            [Description("The AI execution settings.")] OpenAIPromptExecutionSettings? executionSettings = null,
            [Description("The id of the agent, used to retreive the kernel containing services, plugins, and other state for use throughout the operation.")] string? agentId = null,
            [Description("The CancellationToken to monitor for cancellation requests.")] CancellationToken cancellationToken = default
            )
        {
            var connectionName = this.GetAgienceConnectionName();

            var apiKey = await _credentialService.GetCredential(connectionName) ??
                         throw new ArgumentNullException("OpenAI API Key is missing.");

            var chatCompletionService = new OpenAIChatCompletionService("gpt-3.5-turbo", apiKey, null, null, _kernel?.LoggerFactory); // TODO: Options
            return await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, _kernel, cancellationToken);
        }
    }
}