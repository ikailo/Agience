namespace Agience.Core.Models.Messages
{
    public class ChatMessage
    {
        public string? AuthorRole { get; private set; }
        public string? Content { get; private set; }

        public ChatMessage(string authorRole, string content)
        {
            AuthorRole = authorRole;
            Content = content;
        }

    }
}
