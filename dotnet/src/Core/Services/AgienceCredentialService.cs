using System.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Concurrent;
using Agience.Core.Attributes;
using Agience.Core.Extensions;
using Agience.Core.Models.Messages;
using System.Security.Cryptography;
using System.Text.Json;

namespace Agience.Core.Services
{
    public class AgienceCredentialService
    {
        private readonly string _agentId;
        private readonly Authority _authority;
        private readonly Broker _broker;
        private readonly ConcurrentDictionary<string, string> _credentials = new();
        private readonly JsonWebKey _encryptionKey;
        private readonly JsonWebKey _decryptionKey;

        private readonly TopicGenerator _topicGenerator;

        public AgienceCredentialService(string agentId, Authority authority, Broker broker)
        {
            _agentId = agentId;
            _authority = authority;
            _broker = broker;

            _topicGenerator = new TopicGenerator(_authority.Id, _agentId);

            using var rsa = RSA.Create(2048);
            var rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(includePrivateParameters: true))
            {
                KeyId = Guid.NewGuid().ToString()
            };

            // Create the decryption key (with private components)
            _decryptionKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(rsaSecurityKey);

            // Create the encryption key (public only)
            var publicParameters = rsa.ExportParameters(includePrivateParameters: false);
            var publicRsaKey = new RsaSecurityKey(publicParameters) { KeyId = rsaSecurityKey.KeyId };
            _encryptionKey = JsonWebKeyConverter.ConvertFromRSASecurityKey(publicRsaKey);
        }

        public async Task<string?> GetCredential(string name)
        {
            //EnsureCallerHasAccess(name);

            if (_credentials.TryGetValue(name, out var credential))
                return credential;

            await SendCredentialMessageAsync(name);
            while (!_credentials.TryGetValue(name, out credential))
            {
                await Task.Delay(100);
            }
            return credential;
        }

        public void AddEncryptedCredential(string name, string encryptedCredential)
        {
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(encryptedCredential))
                throw new ArgumentException("Invalid credential or name.");

            var decryptedCredential = _decryptionKey.DecryptWithJwk(encryptedCredential);
            _credentials[name] = decryptedCredential;
        }

        private async Task SendCredentialMessageAsync(string credentialName)
        {
            await _broker.PublishAsync(new BrokerMessage
            {
                Type = BrokerMessageType.EVENT,
                Topic = _topicGenerator.PublishToAuthority(),
                Data = new Data
                {
                    { "type", "credential_request" },
                    { "agent_id", _agentId },
                    { "credential_name", credentialName },
                    { "jwk", JsonSerializer.Serialize(_encryptionKey) }
                }
            });
        }

        private void EnsureCallerHasAccess(string connectionName)
        {
            var stackTrace = new StackTrace();
            var frame = stackTrace.GetFrames()?.FirstOrDefault();
            var method = frame?.GetMethod();

            if (method == null)
                throw new UnauthorizedAccessException("Caller information could not be determined.");

            var attribute = method.GetCustomAttributes(typeof(AgienceConnectionAttribute), false)
                .FirstOrDefault() as AgienceConnectionAttribute;

            if (attribute == null || attribute.Name != connectionName)
                throw new UnauthorizedAccessException($"Caller does not have access to the credential: {connectionName}.");
        }
    }
}
