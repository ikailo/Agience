using Duende.IdentityServer.Services;
using Duende.IdentityServer.Services.KeyManagement;
using Duende.IdentityServer.Stores;
using Microsoft.IdentityModel.Tokens;

namespace Agience.Authority.Identity.Services
{
    public class AgienceKeyMaterialService : DefaultKeyMaterialService
    {
        private readonly SemaphoreSlim _lock = new SemaphoreSlim(1, 1);
        private EncryptingCredentials? _encryptingCredentials;

        public AgienceKeyMaterialService(
            IEnumerable<IValidationKeysStore> validationKeysStores,
            IEnumerable<ISigningCredentialStore> signingCredentialStores,
            IAutomaticKeyManagerKeyStore keyManagerKeyStore)
                : base(validationKeysStores, signingCredentialStores, keyManagerKeyStore) { }

        private EncryptingCredentials GenerateEncryptingCredentials()
        {
            // TODO: Persist the encryption key securely and load it here

            var key = new byte[32]; // 256 bits for AES
            using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
            {
                rng.GetBytes(key);
            }

            string encryptionKey = Convert.ToBase64String(key);

            var securityKey = new SymmetricSecurityKey(Convert.FromBase64String(encryptionKey));
            return new EncryptingCredentials(securityKey, SecurityAlgorithms.Aes256KW, SecurityAlgorithms.Aes256CbcHmacSha512);
        }

        internal async Task<EncryptingCredentials> GetEncryptionCredentialsAsync()
        {
            if (_encryptingCredentials == null)
            {
                await _lock.WaitAsync();
                try
                {
                    if (_encryptingCredentials == null) 
                    {
                        _encryptingCredentials = GenerateEncryptingCredentials();
                    }
                }
                finally
                {
                    _lock.Release();
                }
            }
            return _encryptingCredentials;
        }
    }
}