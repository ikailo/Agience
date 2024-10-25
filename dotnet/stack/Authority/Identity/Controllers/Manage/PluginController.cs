using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using Newtonsoft.Json.Linq;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class PluginController : ManageControllerBase
    {
        private readonly IMapper _mapper;

        public PluginController(IAgienceDataAdapter dataAdapter, ILogger<PluginController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        [HttpGet("plugins")]
        public async Task<ActionResult<IEnumerable<Plugin>>> GetPlugins()
        {
            return await HandleGet(async () =>
            {
                var plugins = await _dataAdapter.GetRecordsAsPersonAsync<Models.Plugin>(PersonId);
                return _mapper.Map<IEnumerable<Plugin>>(plugins);
            });
        }

        [HttpGet("plugin/{id}")]
        public async Task<ActionResult<Plugin>> GetPlugin(string id)
        {
            return await HandleGet(async () =>
            {
                var plugin = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Plugin>(id, PersonId);
                return _mapper.Map<Plugin>(plugin);
            });
        }

        [HttpGet("plugins/find")]        
        public async Task<ActionResult<IEnumerable<Plugin>>> GetFindPlugins([FromQuery] string s, [FromQuery] bool p = false)
        {
            return await HandleGet(async () =>
            {
                var plugins = await _dataAdapter.FindPluginsAsPersonAsync(s, p, PersonId);
                return _mapper.Map<IEnumerable<Plugin>>(plugins);
            });
        }

        [HttpPost("plugin")]
        public async Task<ActionResult> PostPlugin([FromBody] Plugin plugin)
        {
            return await HandlePost(async () =>
            {
                var modelPlugin = _mapper.Map<Models.Plugin>(plugin);
                return await _dataAdapter.CreateRecordAsPersonAsync(modelPlugin, PersonId);                
            }, nameof(GetPlugin));
        }

        [HttpPut("plugin")]
        public async Task<IActionResult> PutPlugin([FromBody] Plugin plugin)
        {
            return await HandlePut(async () =>
            {
                var modelPlugin = _mapper.Map<Models.Plugin>(plugin);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelPlugin, PersonId);
            });
        }

        [HttpDelete("plugin/{id}")]
        public async Task<IActionResult> DeletePlugin(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Plugin>(id, PersonId);
            });
        }        

        // FUNCTIONS //

        // Get all Functions for a Plugin
        [HttpGet("plugin/{id}/functions")]
        public async Task<ActionResult<IEnumerable<Function>>> GetPluginFunctions(string id)
        {
            return await HandleGet(async () =>
            {
                var functions = await _dataAdapter.GetRecordsAsPersonAsync<Models.Function>(PersonId);
                return _mapper.Map<IEnumerable<Function>>(functions);
            });
        }

        // Get a Function by Id
        [HttpGet("function/{id}")]
        public async Task<ActionResult<Function>> GetPluginFunction(string id)
        {
            return await HandleGet(async () =>
            {
                var function = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Function>(id, PersonId);
                return _mapper.Map<Function>(function);
            });
        }

        // Create a new root Function for a Plugin
        [HttpPost("plugin/{pluginId}/function")]
        public async Task<ActionResult> PostPluginFunction(string pluginId, [FromBody] Function function)
        {
            return await HandlePost(async () =>
            {
                var modelFunction = _mapper.Map<Models.Function>(function);
                return await _dataAdapter.CreateRecordAsPersonAsync(modelFunction, pluginId, PersonId);
            }, nameof(GetPluginFunctions));
        }

        // Associate an existing Function with a Plugin
        [HttpPut("plugin/{pluginId}/function/{functionId}")]
        public async Task<IActionResult> PutPluginFunction(string pluginId, string functionId)
        {
            return await HandlePut(async () =>
            {
                throw new NotImplementedException();                
            });
        }

        // Update an existing function
        [HttpPut("function")]
        public async Task<IActionResult> PutFunction([FromBody] Function function)
        {
            return await HandlePut(async () =>
            {
                var modelFunction = _mapper.Map<Models.Function>(function);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelFunction, PersonId);
            });
        }

        // Delete an existing function
        [HttpDelete("function/{id}")]
        public async Task<IActionResult> DeleteFunction(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Function>(id, PersonId);
            });
        }

        // CONNECTIONS //

        // Get all Connections for a Plugin
        [HttpGet("plugin/{id}/connections")]
        public async Task<ActionResult<IEnumerable<PluginConnection>>> GetPluginConnections(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<IEnumerable<PluginConnection>>(await _dataAdapter.GetRecordsAsPersonAsync<Models.PluginConnection>(PersonId));
            });
        }

        // Get a Connection by Id
        [HttpGet("connection/{id}")]
        public async Task<ActionResult<PluginConnection>> GetConnection(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<PluginConnection>(await _dataAdapter.GetRecordByIdAsPersonAsync<Models.PluginConnection>(id, PersonId));
            });
        }

        // Create a new Connectionn for a Plugin
        [HttpPost("plugin/{id}/connection")]
        public async Task<ActionResult> PostPluginConnection(string id, [FromBody] PluginConnection connection)
        {
            return await HandlePost(async () =>
            {
                return await _dataAdapter.CreateRecordAsPersonAsync(_mapper.Map<Models.PluginConnection>(connection), id, PersonId);
            }, 
            nameof(GetConnection));
        }

        // Update an existing connection
        [HttpPut("connection")]
        public async Task<IActionResult> PutConnection([FromBody] PluginConnection connection)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.UpdateRecordAsPersonAsync(_mapper.Map<Models.PluginConnection>(connection), PersonId);
            });
        }

        // Delete an existing connection
        [HttpDelete("connection/{id}")]
        public async Task<IActionResult> DeleteConnection(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.PluginConnection>(id, PersonId);
            });
        }
    }
}
