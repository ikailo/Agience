using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Mvc;

namespace Agience.Authority.Manage.Controllers
{
    public class AuthenticationController : Controller
    {
        private readonly ILogger<AuthenticationController> _logger;

        public AuthenticationController(ILogger<AuthenticationController> logger)
        {
            _logger = logger;
        }


        [HttpGet("auth/signin")]
        public IActionResult SignIn([FromQuery] string returnUrl = "/")
        {
            var authProperties = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            _logger.LogInformation("Redirecting to OpenID Connect provider for sign in.");
            return Challenge(authProperties, OpenIdConnectDefaults.AuthenticationScheme);
        }

        [HttpGet("auth/signout")]
        public IActionResult SignOutUser([FromQuery] string returnUrl = "/")
        {
            var authProperties = new AuthenticationProperties
            {
                RedirectUri = returnUrl
            };

            return SignOut(authProperties, new string[]
            {
                OpenIdConnectDefaults.AuthenticationScheme,
                "Cookies"
            });
        }
    }
}
