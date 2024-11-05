using System.ComponentModel;
using Agience.Core.Services;
using Agience.Core.Attributes;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.OpenAI;

namespace Agience.Plugins.Core.OpenAI
{
    [PluginConnection(CONNECTION_NAME)]
    public class ChatCompletionPlugin
    {
        private const string CONNECTION_NAME = "OpenAI";
        private const string HOST_CONNECTION_NAME = "HostOpenAiApiKey";

        private readonly AgienceCredentialService _credentialService;
        private readonly IKernelStore _kernelStore;

        public ChatCompletionPlugin(AgienceCredentialService credentialService, IKernelStore kernelStore)
        {
            _credentialService = credentialService;
            _kernelStore = kernelStore;
        }

        [KernelFunction, Description("Get multiple chat content choices for the prompt and settings.")]
        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(
            [Description("The chat history context.")] ChatHistory chatHistory,
            [Description("The AI execution settings.")] OpenAIPromptExecutionSettings? executionSettings = null,
            //[Description("The Kernel containing services, plugins, and other state for use throughout the operation.")] Kernel? kernel = null,
            [Description("The id of the agent, used to retreive the kernel containing services, plugins, and other state for use throughout the operation.")] string? agentId = null,
            [Description("The CancellationToken to monitor for cancellation requests.")] CancellationToken cancellationToken = default
            )
        {
            var apiKey = _credentialService.GetCredential(CONNECTION_NAME) ?? 
                _credentialService.GetCredential(HOST_CONNECTION_NAME) ?? 
                throw new ArgumentNullException("OpenAI API Key is missing.");

            var kernel = !string.IsNullOrEmpty(agentId) ? _kernelStore.GetKernel(agentId) : throw new ArgumentNullException("agentId is missing.");

            var chatCompletionService = new OpenAIChatCompletionService("gpt-3.5-turbo", apiKey, null, null, kernel?.LoggerFactory); // TODO: Options
            return await chatCompletionService.GetChatMessageContentsAsync(chatHistory, executionSettings, kernel, cancellationToken);
        }
    }
}