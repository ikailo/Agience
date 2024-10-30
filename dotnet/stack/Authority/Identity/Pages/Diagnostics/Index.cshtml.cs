using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;
using Microsoft.AspNetCore.Authorization;
using Agience.Authority.Identity.Filters;


namespace IdentityServerHost.Pages.Diagnostics;

[SecurityHeaders]
[Authorize]
public class Index : PageModel
{
    public ViewModel View { get; set; }
        
    public async Task<IActionResult> OnGet()
    {
        // TODO: Lockdown in production

        //var localAddresses = new string[] { "127.0.0.1", "::1", HttpContext.Connection.LocalIpAddress.ToString() };

        //if (localAddresses.Contains(HttpContext.Connection.RemoteIpAddress.ToString()))
        {
            View = new ViewModel(await HttpContext.AuthenticateAsync());

            return Page();
        }

        //return NotFound();
        
    }
}