namespace Agience.Core
{
    public class MessageAclChecker
    {
        public const string EVENT_PREFIX = "event/";
        public const string CONNECT_PREFIX = "connect/";

        private const string ALL = "0";
        private const string NONE = "-";
        private const string ANY_EXCLUSIVE = "*";
        private const string ANY_INCLUSIVE = "+";
        private const string QUERY = "?";

        private const int READ = 1;
        private const int WRITE = 2;
        private const int READ_WRITE = 3;
        private const int SUBSCRIBE = 4;

        private Func<string, string, string, Task<bool>> _verifyHostSourceTargetRelationships;

        public MessageAclChecker(Func<string, string, string, Task<bool>> verifyHostSourceTargetRelationships)
        {
            _verifyHostSourceTargetRelationships = verifyHostSourceTargetRelationships;
        }

        // TODO: Rework and document all of this

        public async Task<bool> CheckAccessControl(AclCheckRequest? aclRequest, string? hostId, List<string> roles, string? authorityId)
        {
            // TODO: We'll need to lock this down
            if (aclRequest.topic.StartsWith(CONNECT_PREFIX))
            {
                return true;
            }

            // TODO: This just makes it compatible with the current implementation. We should remove this.
            if (aclRequest.topic.StartsWith(EVENT_PREFIX))
            {
                aclRequest.topic = aclRequest.topic.Remove(0, EVENT_PREFIX.Length);
            }

            if (aclRequest == null || string.IsNullOrWhiteSpace(aclRequest.topic) || aclRequest.acc == 0)
            {
                return false;
            }

            if (aclRequest.acc == READ_WRITE)
            {
                return false;
            }

            var masks = GetUserMasks(aclRequest.acc, roles, authorityId, hostId);

            if (await IsTopicAllowed(aclRequest.topic, masks, hostId, aclRequest.acc))
            {
                return true;
            }

            return false;
        }

        private List<string> GetUserMasks(int accessType, List<string> roles, string? authorityId, string? hostId)
        {
            bool isAuthority = roles.Contains("authority");
            bool isHost = roles.Contains("host");

            List<string> masks = new();

            if (isAuthority && authorityId != null)
            {
                if (accessType == READ || accessType == SUBSCRIBE)
                {
                    masks.Add($"{ANY_EXCLUSIVE}/{authorityId}/{NONE}/{NONE}"); // Any -> Authority
                }
                if (accessType == WRITE)
                {
                    masks.Add($"{NONE}/{authorityId}/{ANY_INCLUSIVE}/{ANY_INCLUSIVE}"); // Authority -> Any Host or Agent                    
                }
            }

            if (isHost && hostId != null)
            {
                if (accessType == READ || accessType == SUBSCRIBE)
                {
                    masks.Add($"{NONE}/{authorityId}/{ALL}/{NONE}"); // Authority -> All Hosts
                    masks.Add($"{NONE}/{authorityId}/{hostId}/{NONE}"); // Authority -> Host
                    masks.Add($"{NONE}/{authorityId}/{hostId}/{QUERY}"); // Authority -> Agent
                    masks.Add($"{ANY_EXCLUSIVE}/{authorityId}/{NONE}/{QUERY}"); // Any -> Agent
                }
                if (accessType == WRITE)
                {
                    masks.Add($"{hostId}/{authorityId}/{NONE}/{NONE}"); // Host -> Authority
                    masks.Add($"{QUERY}/{authorityId}/{NONE}/{NONE}"); // Agent -> Authority
                    masks.Add($"{QUERY}/{authorityId}/{NONE}/{QUERY}"); // Agent -> Agent

                }
            }
            return masks;
        }

        private async Task<bool> IsTopicAllowed(string topic, List<string> masks, string? hostId, int accessType)
        {
            foreach (var mask in masks)
            {
                if (CheckMask(topic, mask, accessType))
                {
                    return true;
                }

                if (mask.Contains(QUERY)) // TODO: Subject to "?" injection attack.
                {
                    if (hostId == null) throw new ArgumentNullException(nameof(hostId));

                    if (await CheckQueryMaskAsync(topic, mask, hostId, accessType))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private async Task<bool> CheckQueryMaskAsync(string topic, string mask, string hostId, int accessType)
        {
            var topicParts = topic.Split('/');
            var maskParts = mask.Split('/');

            if (!IsValidTopicAndMask(topicParts, maskParts)) { return false; }

            var sourceId = maskParts[0] == QUERY ? topicParts[0] : null;
            var targetAgentId = maskParts[3] == QUERY ? topicParts[3] : null;

            if (sourceId == NONE || targetAgentId == NONE) { return false; }

            if (accessType == WRITE && sourceId == targetAgentId)
            {
                return false; // Can't send to self
            }

            return await _verifyHostSourceTargetRelationships(hostId, sourceId, targetAgentId);
        }

        private bool CheckMask(string topic, string mask, int accessType)
        {
            var topicParts = topic.Split('/');
            var maskParts = mask.Split('/');

            if (!IsValidTopicAndMask(topicParts, maskParts)) return false;

            if (topicParts[0] == null || maskParts[0] == null) { return false; }

            // First part is the sender id. Read from any sender. Otherwise, the sender id must match the claims.
            if (accessType != READ)
            {
                if (accessType == SUBSCRIBE && topicParts[0] != ANY_INCLUSIVE) { return false; }
                if (accessType == WRITE && topicParts[0] != maskParts[0]) { return false; }
            }

            for (int i = 1; i < maskParts.Length; i++)
            {
                switch (maskParts[i])
                {
                    case ANY_INCLUSIVE:
                        continue;
                    case ANY_EXCLUSIVE when topicParts[i] != ALL:
                        continue;
                    case ALL when topicParts[i] == ALL:
                        continue;
                    case NONE when topicParts[i] == NONE:
                        continue;
                    case var m when m != topicParts[i]:
                        return false;
                }
            }
            return true;
        }

        private bool IsValidTopicAndMask(string[] topicParts, string[] maskParts)
        {
            return topicParts.Length == 4 && maskParts.Length == 4;
        }

        public class AclCheckRequest
        {
            public int acc { get; set; }
            public string clientid { get; set; } = string.Empty;
            public string topic { get; set; } = string.Empty;
        }

        // For logging
        private string PrintConst(int value)
        {
            return value switch
            {
                0 => "0",
                1 => nameof(READ),
                2 => nameof(WRITE),
                3 => nameof(READ_WRITE),
                4 => nameof(SUBSCRIBE),
                _ => "UNKNOWN"
            };
        }
    }
}
