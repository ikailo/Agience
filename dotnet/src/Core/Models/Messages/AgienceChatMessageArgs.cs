using Microsoft.SemanticKernel;

namespace Agience.Core.Models.Messages
{
    public class AgienceChatMessageArgs
    {
        public ChatMessageContent? Message { get; set; }
        public string? AgentId { get; set; }
        public string? LatestMessage => Message?.Content;
    }
}