using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Agience.Authority.Identity.Controllers.Broker
{
    [Route("broker/connect/check")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "host,authority", Policy = "connect")]
    public class BrokerConnectCheckController : ControllerBase
    {
        [HttpPost]
        public IActionResult Post()
        {
            // If we got here, the user is authenticated and authorized.
            return Ok();
        }
    }
}
