using Duende.IdentityServer.Models;
using Duende.IdentityServer.Validation;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;

namespace Agience.Authority.Identity.Validators
{
    public class HostSecretValidator : ISecretValidator
    {
        // TODO: For better security, bind the client Id to the secret. This will prevent a secret from being used by another client.
        // TODO: Create a script that can generate the initial secret for an Authority.

        public Task<SecretValidationResult> ValidateAsync(IEnumerable<Secret> secrets, ParsedSecret parsedSecret)
        {   
            foreach (var secret in secrets)
            {
                var parts = secret.Value.Split('.');

                if (parts.Length == 2)
                {
                    string? hash = parts[0];
                    string? salt = parts[1];
                    string? credential = (string?)parsedSecret?.Credential;
                                        
                    if (!string.IsNullOrEmpty(hash) && !string.IsNullOrEmpty(salt) && !string.IsNullOrEmpty(credential))
                    {
                        if (HashSecret(credential, salt).Equals(secret.Value, StringComparison.Ordinal))
                        {
                            return Task.FromResult(new SecretValidationResult() { Success = true });
                        }                       
                    }
                }
            }
            return Task.FromResult(new SecretValidationResult() { Success = false });
        }

        internal static string HashSecret(string credential, string salt)
        {
            byte[] saltBytes = Base64UrlEncoder.DecodeBytes(salt);
            byte[] credentialBytes = Base64UrlEncoder.DecodeBytes(credential);
            byte[] clientSecretSaltHash = SHA256.Create().ComputeHash(credentialBytes.Concat(saltBytes).ToArray());
            return $"{Base64UrlEncoder.Encode(clientSecretSaltHash)}.{salt}";
        }

        internal static string Random32ByteString()
        {   
            return Base64UrlEncoder.Encode(RandomNumberGenerator.GetBytes(32));
        }

        internal static string EncryptWithJsonWebKey(string plaintext, Microsoft.IdentityModel.Tokens.JsonWebKey jsonWebKey)
        {
            using (RSA rsa = RSA.Create())
            {
                rsa.ImportParameters(new RSAParameters
                {
                    Modulus = Base64UrlEncoder.DecodeBytes(jsonWebKey.N),
                    Exponent = Base64UrlEncoder.DecodeBytes(jsonWebKey.E)
                });

                byte[] plaintextBytes = Encoding.UTF8.GetBytes(plaintext);
                byte[] encryptedBytes = rsa.Encrypt(plaintextBytes, RSAEncryptionPadding.OaepSHA256);

                return Convert.ToBase64String(encryptedBytes);
            }
        }
    }
}
