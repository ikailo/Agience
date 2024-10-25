using Microsoft.AspNetCore.Mvc;
using Agience.SDK;
using Microsoft.AspNetCore.Authorization;
using Agience.Authority.Identity.Data.Adapters;
using Agience.SDK.Models.Entities;

namespace Agience.Authority.Identity.Controllers.Broker
{
    [Route("broker/acl/check")]
    [Authorize(AuthenticationSchemes = "Bearer", Roles = "host,authority", Policy = "connect")]
    public class BrokerAclCheckController : ControllerBase
    {
        private readonly ILogger<BrokerAclCheckController> _logger;
        private readonly IAgienceDataAdapter _dataAdapter;
        private TopicAclChecker _topicAclChecker;

        public BrokerAclCheckController(IAgienceDataAdapter dataAdapter, ILogger<BrokerAclCheckController> logger)
        {
            _logger = logger;
            _dataAdapter = dataAdapter;
            _topicAclChecker = new(VerifyHostSourceTargetRelationships);
        }

        [HttpPost]
        public async Task<ActionResult> CheckAccessControl()
        {
            var aclRequest = await Request.ReadFromJsonAsync<TopicAclChecker.AclCheckRequest>();

            var hostId = User.FindFirst("host_id")?.Value;
            var authorityId = User.FindFirst("authority_id")?.Value;

            List<string> roles = new();
            if (User.IsInRole("authority")) { roles.Add("authority"); }
            if (User.IsInRole("host")) { roles.Add("host"); }

            if (await _topicAclChecker.CheckAccessControl(aclRequest, hostId, roles, authorityId))
            {
                return Ok();
            }

            // TODO: Return better status codes, logging.
            _logger.LogWarning($"Unauthorized CheckAccessControl for {hostId}, {authorityId}");
            return Unauthorized();
        }

        private async Task<bool> VerifyHostSourceTargetRelationships(string hostId, string? sourceId, string? targetAgencyId, string? targetAgentId)
        {
            return await _dataAdapter.VerifyHostSourceTargetRelationships(hostId, sourceId, targetAgencyId, targetAgentId);
        }
    }
}
