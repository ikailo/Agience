using Duende.IdentityServer;
using Duende.IdentityServer.Events;
using Duende.IdentityServer.Services;
using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Agience.Authority.Identity.Filters;
using Agience.Authority.Identity.Models;
using System.Security.Claims;

namespace IdentityServerHost.Pages.ExternalLogin;

[AllowAnonymous]
[SecurityHeaders]
public class Callback : PageModel
{
    private readonly IUserStore<Person> _users;
    private readonly IIdentityServerInteractionService _interaction;
    private readonly ILogger<Callback> _logger;
    private readonly IEventService _events;

    public Callback(
        IIdentityServerInteractionService interaction,
        IEventService events,
        ILogger<Callback> logger,
        IUserStore<Person> users)
    {
        _users = users;
        _interaction = interaction;
        _logger = logger;
        _events = events;
    }

    public async Task<IActionResult> OnGet()
    {
        // read external identity from the temporary cookie        
        var result = await HttpContext.AuthenticateAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);
        if (result?.Succeeded != true)
        {
            throw new Exception("External authentication error");
        }

        var externalUser = result.Principal!;

        if (_logger.IsEnabled(LogLevel.Debug))
        {
            var externalClaims = externalUser.Claims.Select(c => $"{c.Type}: {c.Value}");
            _logger.LogDebug("External claims: {@claims}", externalClaims);
        }

        // lookup our user and external provider info
        // try to determine the unique id of the external user (issued by the provider)
        // the most common claim type for that are the sub claim and the NameIdentifier
        // depending on the external provider, some other claim type might be used
        var userIdClaim = externalUser.FindFirst(JwtClaimTypes.Subject) ??
                          externalUser.FindFirst(ClaimTypes.NameIdentifier) ??
                          throw new Exception("Unknown userid");

        var providerId = result?.Principal?.Identity?.AuthenticationType ?? throw new Exception("ProviderId not returned.");

        var providerPersonId = userIdClaim.Value;

        // find external user
        var user = await _users.FindByIdAsync(Person.CombineProviderAndId(providerId, providerPersonId), default); // TODO: Implement Cancellation Token

        if (user == null)
        {
            // this might be where you might initiate a custom workflow for user registration
            // in this sample we don't show how that would be done, as our sample implementation
            // simply auto-provisions new external user
            //
            // remove the user id claim so we don't include it as an extra claim if/when we provision the user
            //var claims = externalUser.Claims.ToList();
            //claims.Remove(userIdClaim);

            user = new Person()
            {
                ProviderId = providerId,
                ProviderPersonId = providerPersonId,
                Email = externalUser.Claims.First(c => c.Type == ClaimTypes.Email)?.Value,
                FirstName = externalUser.Claims.First(c => c.Type == ClaimTypes.GivenName)?.Value,
                LastName = externalUser.Claims.First(c => c.Type == ClaimTypes.Surname)?.Value,
                LastLogin = DateTime.UtcNow
            };

            var createResult = await _users.CreateAsync(user, default);

            if (!createResult.Succeeded)
            {
                _logger.LogError(createResult.Errors.First().Description);

                throw new Exception("User not provisioned.");
            }

            //user = await _users.FindByIdAsync(Person.CombineProviderAndId(provider, providerUserId), default); // TODO: Probably we can skip this and just use the newly created users.
        }
        else
        {
            user.LastLogin = DateTime.UtcNow;
            _ = await _users.UpdateAsync(user, default); // TODO: Await not necessary.
        }

        // this allows us to collect any additional claims or properties
        // for the specific protocols used and store them in the local auth cookie.
        // this is typically used to store data needed for signout from those protocols.
        var additionalLocalClaims = new List<Claim>();

        if (user?.Name != null) { additionalLocalClaims.Add(new Claim(JwtClaimTypes.Name, user!.Name)); }
        if (user?.Email != null) { additionalLocalClaims.Add(new Claim(JwtClaimTypes.Email, user!.Email)); }
        if (user?.FirstName != null) { additionalLocalClaims.Add(new Claim(JwtClaimTypes.GivenName, user!.FirstName)); }
        if (user?.LastName != null) { additionalLocalClaims.Add(new Claim(JwtClaimTypes.FamilyName, user!.LastName)); }

        var localSignInProps = new AuthenticationProperties();
        CaptureExternalLoginContext(result, additionalLocalClaims, localSignInProps);

        // issue authentication cookie for user
        var isuser = new IdentityServerUser(user!.Id!)
        {
            DisplayName = user.Name,
            IdentityProvider = providerId,
            AdditionalClaims = additionalLocalClaims
        };

        await HttpContext.SignInAsync(isuser, localSignInProps);

        // delete temporary cookie used during external authentication
        await HttpContext.SignOutAsync(IdentityServerConstants.ExternalCookieAuthenticationScheme);

        // retrieve return URL
        var returnUrl = result.Properties?.Items["returnUrl"] ?? "~/";

        // check if external login is in the context of an OIDC request
        var context = await _interaction.GetAuthorizationContextAsync(returnUrl);
        await _events.RaiseAsync(new UserLoginSuccessEvent(providerId, providerPersonId, user.Id, user.Name, true, context?.Client.ClientId));

        if (context != null)
        {
            if (context.IsNativeClient())
            {
                // The client is native, so this change in how to
                // return the response is for better UX for the end user.
                return this.LoadingPage(returnUrl);
            }
        }

        return Redirect(returnUrl);
    }

    // if the external login is OIDC-based, there are certain things we need to preserve to make logout work
    // this will be different for WS-Fed, SAML2p or other protocols
    private void CaptureExternalLoginContext(AuthenticateResult externalResult, List<Claim> localClaims, AuthenticationProperties localSignInProps)
    {
        // if the external system sent a session id claim, copy it over
        // so we can use it for single sign-out
        var sid = externalResult.Principal?.Claims.FirstOrDefault(x => x.Type == JwtClaimTypes.SessionId);
        if (sid != null)
        {
            localClaims.Add(new Claim(JwtClaimTypes.SessionId, sid.Value));
        }

        // if the external provider issued an id_token, we'll keep it for signout
        var idToken = externalResult.Properties?.GetTokenValue("id_token");
        if (idToken != null)
        {
            localSignInProps.StoreTokens(new[] { new AuthenticationToken { Name = "id_token", Value = idToken } });
        }
    }
}