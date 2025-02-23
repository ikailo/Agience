using Duende.IdentityServer;
using Duende.IdentityServer.Models;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication.Google;
using System.Security.Claims;

namespace Agience.Authority.Identity.Services
{
    public class ProfileService : IProfileService
    {
        public Task GetProfileDataAsync(ProfileDataRequestContext context)
        {
            var subjectClaims = context.Subject.Claims;
            var issuedClaims = context.IssuedClaims;

            if (subjectClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.IdentityProvider)?.Value == GoogleDefaults.AuthenticationScheme && context.Caller == IdentityServerConstants.ProfileDataCallers.ClaimsProviderAccessToken)
            {
                issuedClaims.Add(new Claim(JwtClaimTypes.Role, "user"));
                issuedClaims.Add(new Claim(JwtClaimTypes.Scope, "manage"));

                var subClaim = subjectClaims.FirstOrDefault(c => c.Type == JwtClaimTypes.Subject);

                if (subClaim != default(Claim))
                {
                    issuedClaims.Add(new Claim("user_id", subClaim.Value));
                }
            }

            context.AddRequestedClaims(subjectClaims);

            return Task.CompletedTask;

        }

        public Task IsActiveAsync(IsActiveContext context)
        {
            context.IsActive = context.Subject.Identity?.IsAuthenticated ?? false;

            return Task.CompletedTask;
        }
    }
}
