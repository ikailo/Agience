using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Agience.Core.Extensions
{
    public static class JsonWebKeyExtensions
    {
        public static string EncryptWithJwk(this JsonWebKey jwk, string data)
        {
            if (jwk == null)
                throw new ArgumentNullException(nameof(jwk), "JsonWebKey cannot be null.");

            if (string.IsNullOrWhiteSpace(data))
                throw new ArgumentException("Data to encrypt cannot be null or empty.", nameof(data));

            if (string.IsNullOrWhiteSpace(jwk.N) || string.IsNullOrWhiteSpace(jwk.E))
                throw new InvalidOperationException("JWK must contain both Modulus (N) and Exponent (E) for encryption.");

            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E)
            });

            var encryptedBytes = rsa.Encrypt(
                System.Text.Encoding.UTF8.GetBytes(data),
                RSAEncryptionPadding.OaepSHA256
            );

            return Convert.ToBase64String(encryptedBytes);
        }

        public static string DecryptWithJwk(this JsonWebKey jwk, string encryptedData)
        {
            if (jwk == null)
                throw new ArgumentNullException(nameof(jwk));

            if (string.IsNullOrWhiteSpace(encryptedData))
                throw new ArgumentException("Encrypted data cannot be null or empty.", nameof(encryptedData));

            if (string.IsNullOrWhiteSpace(jwk.D))
                throw new InvalidOperationException("Decryption requires a private key (D component).");

            using var rsa = RSA.Create();
            rsa.ImportParameters(new RSAParameters
            {
                Modulus = Base64UrlEncoder.DecodeBytes(jwk.N),
                Exponent = Base64UrlEncoder.DecodeBytes(jwk.E),
                D = Base64UrlEncoder.DecodeBytes(jwk.D),
                P = jwk.P != null ? Base64UrlEncoder.DecodeBytes(jwk.P) : null,
                Q = jwk.Q != null ? Base64UrlEncoder.DecodeBytes(jwk.Q) : null,
                DP = jwk.DP != null ? Base64UrlEncoder.DecodeBytes(jwk.DP) : null,
                DQ = jwk.DQ != null ? Base64UrlEncoder.DecodeBytes(jwk.DQ) : null,
                InverseQ = jwk.QI != null ? Base64UrlEncoder.DecodeBytes(jwk.QI) : null,
            });

            var decryptedBytes = rsa.Decrypt(
                Base64UrlEncoder.DecodeBytes(encryptedData),
                RSAEncryptionPadding.OaepSHA256
            );

            return System.Text.Encoding.UTF8.GetString(decryptedBytes);
        }
    }
}
