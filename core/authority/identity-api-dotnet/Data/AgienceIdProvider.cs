using System.Security.Cryptography;
using System.Text;

namespace Agience.Authority.Identity.Data
{
    public class AgienceIdProvider
    {
        private static int _counter = 0;
        private static readonly object _lockObj = new object();
        private readonly string _authorityUri;

        public AgienceIdProvider(string authorityUri)
        {
            _authorityUri = authorityUri;
        }

        public string GenerateId(string agienceEntityType)
        {
            lock (_lockObj)
            {
                string seed =
                    _authorityUri +
                    agienceEntityType +
                    DateTime.UtcNow.ToString("yyyyMMddHHmmssfff") +
                    _counter++ +
                     Random.Shared.Next(int.MaxValue);

                byte[] digest = GenerateSha256Digest(seed);
                return Base64EncodeUrlSafe(digest);
            }
        }

        private static byte[] GenerateSha256Digest(string input)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                return sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(input));
            }
        }

        private static string Base64EncodeUrlSafe(byte[] input)
        {
            return Convert.ToBase64String(input).Replace('+', '-').Replace('/', '_').Replace("=", string.Empty);
        }

        public static byte[] Base64DecodeUrlSafe(string input)
        {
            string base64 = input.Replace('-', '+').Replace('_', '/');

            int mod = base64.Length % 4;
            if (mod > 0)
            {
                base64 = base64.PadRight(base64.Length + (4 - mod), '=');
            }

            return Convert.FromBase64String(base64);
        }
    }
}
