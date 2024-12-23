using Microsoft.AspNetCore.Mvc;
using Agience.Core;
using Microsoft.AspNetCore.Authorization;
using Agience.Core.Models.Entities;
using Agience.Authority.Identity.Data.Repositories;

namespace Agience.Authority.Identity.Controllers.Broker
{
    [Route("broker/acl/check")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "host,authority", Policy = "connect")]
    public class BrokerAclCheckController : ControllerBase
    {
        private readonly ILogger<BrokerAclCheckController> _logger;
        //private readonly HostRepository _hostRepository;
        private MessageAclChecker _topicAclChecker;

        public BrokerAclCheckController(ILogger<BrokerAclCheckController> logger)
        {
            _logger = logger;
            //_hostRepository = hostRepository;
            _topicAclChecker = new(VerifyHostSourceTargetRelationships);
        }

        [HttpPost]
        public async Task<ActionResult> CheckAccessControl()
        {
            var aclRequest = await Request.ReadFromJsonAsync<MessageAclChecker.AclCheckRequest>();

            var hostId = User.FindFirst("host_id")?.Value;
            var authorityId = User.FindFirst("authority_id")?.Value;

            List<string> roles = new();
            if (User.IsInRole("authority")) { roles.Add("authority"); }
            if (User.IsInRole("host")) { roles.Add("host"); }

            _logger.LogDebug($"CheckAccessControl: {aclRequest.acc},{aclRequest.topic} | {hostId}, {string.Join(",", roles)}, {authorityId}");

            if (await _topicAclChecker.CheckAccessControl(aclRequest, hostId, roles, authorityId))
            {
                return Ok();
            }

            // TODO: Return better status codes, logging.
            _logger.LogWarning($"Unauthorized CheckAccessControl for {hostId}, {authorityId}");
            return Unauthorized();
        }

        private async Task<bool> VerifyHostSourceTargetRelationships(string hostId, string sourceAgentId, string targetAgentId)
        {
            return true; // TODO: Fix this. It needs to be implemented.
        }
    }
}
