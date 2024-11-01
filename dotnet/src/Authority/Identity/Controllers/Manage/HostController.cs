using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Host = Agience.Authority.Models.Manage.Host;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using AutoMapper;
using System.Text.Json.Serialization;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class HostController : ManageControllerBase
    {
        private readonly IMapper _mapper;

        public HostController(IAgienceDataAdapter dataAdapter, ILogger<AgentController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        [HttpGet("hosts")]
        public async Task<ActionResult<IEnumerable<Host>>> GetHosts([FromQuery] bool p = false)
        {
            return await HandleGet(async () =>
            {
                var hosts = await _dataAdapter.GetRecordsAsPersonAsync<Models.Host>(PersonId, p);
                return _mapper.Map<IEnumerable<Host>>(hosts);
            });
        }

        [HttpGet("host/{id}")]
        public async Task<ActionResult<Host>> GetHost(string id)
        {
            return await HandleGet(async () =>
            {
                var host = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Host>(id, PersonId);
                return _mapper.Map<Host>(host);
            });
        }

        [HttpPost("host")]
        public async Task<ActionResult> PostHost([FromBody] Host host)
        {
            return await HandlePost(async () =>
            {
                var modelHost = _mapper.Map<Models.Host>(host);
                return await _dataAdapter.CreateRecordAsPersonAsync(modelHost, PersonId);                
            }, nameof(GetHost));
        }

        [HttpPut("host")]
        public async Task<IActionResult> PutHost([FromBody] Host host)
        {
            return await HandlePut(async () =>
            {
                var modelHost = _mapper.Map<Models.Host>(host);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelHost, PersonId);
            });
        }

        [HttpDelete("host/{id}")]
        public async Task<IActionResult> DeleteHost(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Host>(id, PersonId);
            });
        }

        // PLUGINS //

        [HttpPut("host/{hostId}/plugin/{pluginId}")]
        public async Task<IActionResult> PutHostPlugin(string hostId, string pluginId)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.AddPluginToHostAsPersonAsync(hostId, pluginId, PersonId);
            });
        }

        [HttpDelete("host/{hostId}/plugin/{pluginId}")]
        public async Task<IActionResult> DeleteHostPlugin(string hostId, string pluginId)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.RemovePluginFromHostAsPersonAsync(hostId, pluginId, PersonId);
            });
        }

        // KEYS //

        [HttpPost("host/{hostId}/key/generate")]
        public async Task<ActionResult<Key>> GenerateHostKey(string hostId, [FromBody] GenerateKeyRequest generateKeyRequest)
        {
            _logger.LogInformation("Generating key for host {hostId}", hostId);

            return await HandleGet(async () =>
            {
                var modelHostKey = await _dataAdapter.GenerateHostKeyAsPersonAsync(hostId, generateKeyRequest.Name, generateKeyRequest.JsonWebKey, PersonId);
                return _mapper.Map<Key>(modelHostKey);
            });
        }

        public class GenerateKeyRequest
        {
            [JsonPropertyName("name")]
            public string Name { get; set; } = string.Empty;

            [JsonPropertyName("json_web_key")]
            public JsonWebKey? JsonWebKey { get; set; }
        }

        [HttpPut("key")]
        public async Task<IActionResult> PutKey([FromBody] Key key)
        {
            return await HandlePut(async () =>
            {
                var modelHostKey = _mapper.Map<Models.Key>(key);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelHostKey, PersonId);
            });
        }

        [HttpDelete("key/{id}")]
        public async Task<IActionResult> DeleteKey(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Key>(id, PersonId);
            });
        }
    }
}
