using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Data.Repositories;
using AutoMapper;
using Agience.Authority.Identity.Models;
using Host = Agience.Authority.Identity.Models.Host;
using ManageModel = Agience.Authority.Models.Manage;
using Agience.Authority.Identity.Validators;
using Microsoft.IdentityModel.Tokens;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class HostController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public HostController(ILogger<HostController> logger, RecordsRepository repository, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // *** HOST *** //

        [HttpPost("host")]
        public async Task<ActionResult<ManageModel.Host>> CreateHost([FromBody] Host host)
        {
            return await HandlePost(async () =>
            {
                var createdHost = await _repository.CreateRecordAsPersonAsync(_mapper.Map<Host>(host), PersonId);
                return _mapper.Map<ManageModel.Host>(createdHost);
            }, nameof(GetHostById), "hostId");
        }

        [HttpGet("hosts")]
        public async Task<ActionResult<IEnumerable<ManageModel.Host>>> GetHosts([FromQuery] string? search = null, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                if (string.IsNullOrEmpty(search))
                {
                    var hosts = await _repository.GetRecordsAsPersonAsync<Host>(PersonId, all);
                    return _mapper.Map<IEnumerable<ManageModel.Host>>(hosts);
                }

                var searchResults = await _repository.SearchRecordsAsPersonAsync<Host>(
                    new[] { "Name", "Description" },
                    search,
                    PersonId,
                    all
                );
                return _mapper.Map<IEnumerable<ManageModel.Host>>(searchResults);
            });
        }

        [HttpGet("host/{hostId}")]
        public async Task<ActionResult<ManageModel.Host>> GetHostById(string hostId)
        {
            return await HandleGet(async () =>
            {
                var host = await _repository.GetRecordByIdAsPersonAsync<Host>(hostId, PersonId);

                if (host == null)
                {
                    throw new KeyNotFoundException("Host not found.");
                }

                return _mapper.Map<ManageModel.Host>(host);
            });
        }



        [HttpPut("host/{hostId}")]
        public async Task<IActionResult> UpdateHost(string hostId, [FromBody] Host host)
        {
            return await HandlePut(async () =>
            {
                if (host?.Id == null)
                    throw new ArgumentNullException("Host Id is required.");

                if (host.Id != null && !host.Id.Equals(hostId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                await _repository.UpdateRecordAsPersonAsync(host, PersonId);
            });
        }

        [HttpDelete("host/{hostId}")]
        public async Task<IActionResult> DeleteHost(string hostId)
        {
            return await HandleDelete(async () =>
            {
                var success = await _repository.DeleteRecordAsPersonAsync<Host>(hostId, PersonId);
                if (!success)
                {
                    throw new KeyNotFoundException("Host not found or could not be deleted.");
                }
            });
        }

        // *** HOST_PLUGIN //

        [HttpGet("host/{hostId}/plugins")]
        public async Task<ActionResult<IEnumerable<ManageModel.Plugin>>> GetPluginsForHost(string hostId)
        {
            return await HandleGet(async () =>
            {
                var plugins = await _repository.GetChildRecordsWithJoinAsPersonAsync<Host, Plugin, HostPlugin>("HostId", "PluginId", hostId, PersonId);
                return _mapper.Map<IEnumerable<ManageModel.Plugin>>(plugins);
            });
        }

        [HttpPost("host/{hostId}/plugin/{pluginId}")]
        public async Task<IActionResult> AddPluginToHost(string hostId, string pluginId, bool all = false)
        {
            return await HandlePut(async () =>
            {
                var host = await _repository.GetRecordByIdAsPersonAsync<Host>(hostId, PersonId);

                if (host == null)
                    throw new KeyNotFoundException("Host not found.");

                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId, all);

                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                await _repository.CreateRecordAsSystemAsync(new HostPlugin
                {
                    HostId = hostId,
                    PluginId = pluginId
                });
            });
        }

        [HttpDelete("host/{hostId}/plugin/{pluginId}")]
        public async Task<IActionResult> RemovePluginFromHost(string hostId, string pluginId)
        {
            return await HandleDelete(async () =>
            {
                var host = await _repository.GetRecordByIdAsPersonAsync<Host>(hostId, PersonId);

                if (host == null)
                    throw new KeyNotFoundException("Host not found.");

                var hostPlugin = await _repository.QueryRecordsAsSystemAsync<HostPlugin>(
                    new() { { "HostId", hostId }, { "PluginId", pluginId } }
                );

                if (!hostPlugin.Any())
                    throw new InvalidOperationException("Plugin is not associated with Host.");

                await _repository.DeleteRecordAsSystemAsync<HostPlugin>(hostPlugin.First().Id!);
            });
        }

        // *** KEY *** //

        [HttpPost("host/{hostId}/key/generate")]
        public async Task<ActionResult<ManageModel.Key>> GenerateKeyForHost(string hostId, [FromBody] Key key, [FromQuery] JsonWebKey? jwk = null)
        {
            return await HandlePost(async () =>
            {

                if (string.IsNullOrEmpty(key.Name))
                    throw new InvalidOperationException("Query parameter 'name' is required.");

                var host = await _repository.GetRecordByIdAsPersonAsync<Host>(hostId, PersonId);

                if (host == null)
                    throw new KeyNotFoundException("Host not found.");

                key.HostId = hostId;

                var secret = HostSecretValidator.Random32ByteString();

                key.SaltedValue = HostSecretValidator.HashSecret(secret, HostSecretValidator.Random32ByteString());

                key = await _repository.CreateRecordAsSystemAsync(key);

                if (jwk == null || string.IsNullOrEmpty(jwk.Kty))
                {
                    key.Value = secret;
                    key.IsEncrypted = false;
                }
                else
                {
                    key.Value = HostSecretValidator.EncryptWithJsonWebKey(secret, jwk);
                    key.IsEncrypted = true;
                }

                return _mapper.Map<ManageModel.Key>(key);

            }, nameof(GetKeyById), "keyId");
        }

        [HttpGet("host/{hostId}/keys")]
        public async Task<ActionResult<IEnumerable<ManageModel.Key>>> GetKeysForHost(string hostId)
        {
            return await HandleGet(async () =>
            {
                var keys = await _repository.GetChildRecordsAsPersonAsync<Host, Key>("HostId", hostId, PersonId);

                foreach (var key in keys)
                {
                    // Secrets only get sent when the key is first generated
                    key.SaltedValue = null;
                }

                return _mapper.Map<IEnumerable<ManageModel.Key>>(keys);
            });
        }

        [HttpGet("key/{keyId}")]
        [HttpGet("host/{hostId}/key/{keyId}")]
        public async Task<ActionResult<ManageModel.Key>> GetKeyById(string keyId, string? hostId = null)
        {
            return await HandleGet(async () =>
            {
                Key? key = null;

                if (!string.IsNullOrEmpty(hostId))
                {
                    key = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", hostId, keyId, PersonId);
                }
                else
                {
                    key = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", keyId, PersonId);
                }

                if (key == null)
                    throw new KeyNotFoundException("Key not found.");

                // Secrets only get sent when the key is first generated
                key.SaltedValue = null;

                return _mapper.Map<ManageModel.Key>(key);
            });
        }

        [HttpPut("key/{keyId}")]
        [HttpPut("host/{hostId}/key/{keyId}")]
        public async Task<IActionResult> UpdateKey(string keyId, [FromBody] Key key, string? hostId = null)
        {
            return await HandlePut(async () =>
            {
                if (key?.Id == null)
                    throw new ArgumentNullException("Key Id is required.");

                if (key.Id != null && !key.Id.Equals(keyId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                Key? dbkey = null;

                if (!string.IsNullOrEmpty(hostId))
                {
                    if (key.HostId == null || key.HostId == hostId)
                    {
                        dbkey = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", hostId, keyId, PersonId);
                    }
                }
                else
                {
                    dbkey = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", keyId, PersonId);
                }

                if (dbkey == null)
                    throw new KeyNotFoundException("Key not found.");


                await _repository.UpdateRecordAsSystemAsync(key);
            });
        }

        [HttpDelete("key/{keyId}")]
        [HttpDelete("host/{hostId}/key/{keyId}")]
        public async Task<IActionResult> DeleteKey(string keyId, string? hostId = null)
        {
            return await HandleDelete(async () =>
            {
                Key? key = null;

                if (!string.IsNullOrEmpty(hostId))
                {
                    // Check for key under a specific host
                    key = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", hostId, keyId, PersonId);
                }
                else
                {
                    // Check for key without a specific host
                    key = await _repository.GetChildRecordByIdAsPersonAsync<Host, Key>("HostId", keyId, PersonId);
                }

                if (key == null)
                {
                    throw new KeyNotFoundException("Key not found.");
                }

                var success = await _repository.DeleteRecordAsSystemAsync<Key>(key.Id);

                if (!success)
                {
                    throw new KeyNotFoundException("Key not found or could not be deleted.");
                }
            });
        }

    }
}
