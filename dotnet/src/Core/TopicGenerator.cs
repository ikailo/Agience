namespace Agience.Core
{
    public class TopicGenerator
    {

        public const string EVENT_PREFIX =  "event/";
        public const string CONNECT_PREFIX = "connect/";

        private readonly string _authorityId;
        private readonly string _senderId;

        public TopicGenerator(string authorityId, string senderId)
        {
            _authorityId = authorityId ?? throw new ArgumentNullException(nameof(authorityId));
            _senderId = senderId == authorityId ? "-" : senderId ?? throw new ArgumentNullException(nameof(senderId));
        }

        public string ConnectTo(string topic)
        {
            return $"{CONNECT_PREFIX}{topic}";
        }

        public string SubscribeAs(string? hostId, string? agentId)
        {
            return $"{EVENT_PREFIX}+/{_authorityId}/{hostId ?? "-"}/{agentId ?? "-"}";
        }

        public string PublishTo(string? hostId, string? agentId)
        {
            return $"{EVENT_PREFIX}{_senderId}/{_authorityId}/{hostId ?? "-"}/{agentId ?? "-"}";
        }


        public string SubscribeAsAgent()
        {
            return SubscribeAs(null, _senderId);
        }

        public string PublishToAgent(string? agentId)
        {
            return PublishTo(null, agentId);
        }

        public string PublishToHost(string hostId)
        {
            return PublishTo(hostId, null);
        }

        public string SubscribeAsHost()
        {
            return SubscribeAs(_senderId, null);
        }

        public string PublishToAuthority()
        {
            return PublishTo(null, null);
        }

        public string SubscribeAsAuthority()
        {
            return SubscribeAs(null, null);
        }
    }
}
