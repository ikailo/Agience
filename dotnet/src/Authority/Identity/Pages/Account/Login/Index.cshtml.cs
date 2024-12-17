using Duende.IdentityServer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Agience.Authority.Identity.Filters;

namespace IdentityServerHost.Pages.Login;

[SecurityHeaders]
[AllowAnonymous]
public class Index : PageModel
{
    private readonly IIdentityServerInteractionService _interaction;

    public Index(IIdentityServerInteractionService interaction)
    {
        _interaction = interaction;
    }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        // Generate a nonce for the Content-Security-Policy header
        var nonce = Guid.NewGuid().ToString("N");
        Response.Headers.Add("Content-Security-Policy", $"script-src 'self' 'nonce-{nonce}';");

        // Ensure returnUrl is valid or default to root
        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        if (!Url.IsLocalUrl(returnUrl) && !_interaction.IsValidReturnUrl(returnUrl))
        {
            // Invalid returnUrl scenario
            throw new Exception("Invalid return URL");
        }

        // Automatically redirect to the external login challenge for Google
        var googleChallengeUrl = $"/ExternalLogin/Challenge?scheme=Google&returnUrl={Uri.EscapeDataString(returnUrl)}";
        return Redirect(googleChallengeUrl);
    }
}
