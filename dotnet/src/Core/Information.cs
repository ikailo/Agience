using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace Agience.SDK
{
    public class Information
    {
        public string Id { get; }
        public string? ParentId { get; set; }

        public Data? Input { get; internal set; }
        public string? InputAgentId { get; internal set; }
        public string? InputTimestamp { get; internal set; }

        public Data? Output { get; internal set; }
        public string? OutputAgentId { get; internal set; }
        public string? OutputTimestamp { get; internal set; }

        public string? FunctionId { get; internal set; }

        public Information(string? parentId = null)
        {
            using var sha256 = SHA256.Create();

            Id = Base64UrlEncoder.Encode(sha256.ComputeHash(Guid.NewGuid().ToByteArray()));
            ParentId = parentId;
        }
    }
}
