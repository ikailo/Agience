using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;

namespace Agience.Core.Services
{
    public class AgienceChatCompletionService : IChatCompletionService
    {
        public IReadOnlyDictionary<string, object?> Attributes => throw new NotImplementedException();

        private readonly KernelFunction _chatCompletionFunction;

        public AgienceChatCompletionService(KernelFunction chatCompletionFunction)
        {
            _chatCompletionFunction = chatCompletionFunction;
        }

        public async Task<IReadOnlyList<ChatMessageContent>> GetChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            var args = new KernelArguments(executionSettings);

            args["chatHistory"] = chatHistory;
            args["executionSettings"] = executionSettings;
            args["kernel"] = kernel;
            args["cancellationToken"] = cancellationToken;

            // TODO: We need to ensure that the Chat Completion function will return a list of ChatMessageContent. This will break otherwise.
            // TODO: Support other formats for input/output of the ChatCompletionFunction.

            return await _chatCompletionFunction.InvokeAsync<IReadOnlyList<ChatMessageContent>>(kernel, args, cancellationToken) ?? new List<ChatMessageContent>();
        }

        public IAsyncEnumerable<StreamingChatMessageContent> GetStreamingChatMessageContentsAsync(ChatHistory chatHistory, PromptExecutionSettings? executionSettings = null, Kernel? kernel = null, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}