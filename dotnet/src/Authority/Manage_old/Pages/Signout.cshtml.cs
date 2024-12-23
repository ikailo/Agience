using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Agience.Authority.Manage.Pages;

public class SignoutModel : PageModel
{
    public async Task<IActionResult> OnGet()
    {
        var authenticateResult = await HttpContext.AuthenticateAsync();

        if (authenticateResult.Succeeded && authenticateResult.Principal != null)
        {
            var idToken = authenticateResult.Properties.GetTokenValue("id_token");

            return SignOut(new AuthenticationProperties()
            {
                Parameters =
                {
                    { OpenIdConnectParameterNames.IdTokenHint, idToken }
                }
            },
            CookieAuthenticationDefaults.AuthenticationScheme,
            OpenIdConnectDefaults.AuthenticationScheme
            );
        }

        return SignOut();
    }
}