using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Agience.Authority.Manage.Web.Models;

namespace Agience.Authority.Manage.Web.Pages
{

    public class DiagnosticsModel : PageModel
    {
        public Diagnostic? Diagnostic { get; set; }

        private readonly IWebHostEnvironment _env;

        public DiagnosticsModel(IWebHostEnvironment env)
        {
            _env = env;
        }

        public async Task<IActionResult> OnGet()
        {
            if (_env.IsProduction())
            {
                return NotFound();
            }

            Diagnostic = new Diagnostic(await HttpContext.AuthenticateAsync());

            return Page();
        }
    }
}