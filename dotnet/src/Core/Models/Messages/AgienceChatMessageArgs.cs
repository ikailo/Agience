using Microsoft.SemanticKernel;

namespace Agience.Core.Models.Messages
{
    public class AgienceChatMessageArgs
    {
        public string AgencyId { get; set; }
        public ChatMessageContent Message { get; set; }
        public string AgentId { get; set; }
    }
}