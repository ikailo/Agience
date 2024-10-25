using Duende.IdentityServer.Services;
using Duende.IdentityServer.Stores;
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
    public string Nonce { get; private set; }
    public string ReturnUrl { get; private set; }

    public Index(IIdentityServerInteractionService interaction)
    {
        _interaction = interaction;
    }

    public async Task<IActionResult> OnGet(string returnUrl)
    {
        // TODO: Provide the correct link to the website login page

        Nonce = Guid.NewGuid().ToString("N");
        Response.Headers.Add("Content-Security-Policy", $"script-src 'self' 'nonce-{Nonce}';");

        if (string.IsNullOrEmpty(returnUrl)) returnUrl = "~/";

        // validate returnUrl - either it is a valid OIDC URL or back to a local page
        if (Url.IsLocalUrl(returnUrl) == false && _interaction.IsValidReturnUrl(returnUrl) == false)
        {
            // user might have clicked on a malicious link - should be logged
            throw new Exception("invalid return URL");
        }

        ReturnUrl = returnUrl;
        
        return Page();
    }
}