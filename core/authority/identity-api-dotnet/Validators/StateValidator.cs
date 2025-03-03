using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using Agience.Authority.Identity.Services;

namespace Agience.Authority.Identity.Validators
{
    public class StateValidator
    {
        private readonly AgienceKeyMaterialService _keyMaterialService;

        public StateValidator(AgienceKeyMaterialService keyMaterialService)
        {
            _keyMaterialService = keyMaterialService;
        }

        public async Task<string> GenerateStateAsync(string userId, Dictionary<string, string> additionalClaims)
        {
            var handler = new JwtSecurityTokenHandler();
            var signingCredentials = await _keyMaterialService.GetSigningCredentialsAsync();
            var encryptingCredentials = await _keyMaterialService.GetEncryptionCredentialsAsync();

            var claims = new List<Claim> { new Claim("user_id", userId) };
            claims.AddRange(additionalClaims.Select(kvp => new Claim(kvp.Key, kvp.Value)));

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(10),
                SigningCredentials = signingCredentials,
                EncryptingCredentials = encryptingCredentials
            };

            var token = handler.CreateToken(descriptor);
            return handler.WriteToken(token);
        }

        public async Task<ClaimsPrincipal> ValidateState(string state, string currentUserId)
        {
            var handler = new JwtSecurityTokenHandler();
            var keyInfos = await _keyMaterialService.GetValidationKeysAsync();
            var keys = keyInfos.Select(ki => ki.Key).ToList();

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKeyResolver = (token, securityToken, identifier, parameters) => keys,
                ValidateIssuer = false,
                ValidateAudience = false,
                ClockSkew = TimeSpan.Zero,
                TokenDecryptionKey = (await _keyMaterialService.GetEncryptionCredentialsAsync()).Key
            };

            try
            {
                var principal = handler.ValidateToken(state, validationParameters, out var validatedToken);
                var userIdClaim = principal.FindFirst("user_id")?.Value;

                if (userIdClaim == null || userIdClaim != currentUserId)
                {
                    throw new SecurityTokenException("Invalid user_id in state.");
                }

                return principal;
            }
            catch (Exception ex)
            {
                // Log or handle exception
                throw new SecurityTokenValidationException("State validation failed.", ex);
            }
        }
    }
}
