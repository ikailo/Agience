using Microsoft.AspNetCore.Mvc;
using ManageModel = Agience.Authority.Models.Manage;
using Agience.Authority.Identity.Models;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;
using System.Reflection.Metadata.Ecma335;
using Agience.Authority.Identity.Services;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class PluginController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly PluginImportService _pluginImportService;        
        private readonly IMapper _mapper;
        //private readonly string _workspacePath;

        public PluginController(ILogger<PluginController> logger, RecordsRepository repository, PluginImportService pluginImportService, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _pluginImportService = pluginImportService;
            _mapper = mapper;
        }

        // *** PLUGIN *** //

        [HttpPost("plugin")]
        public async Task<ActionResult<ManageModel.Plugin>> CreatePlugin([FromBody] Plugin plugin)
        {
            return await HandlePost(async () =>
            {
                var createdPlugin = await _repository.CreateRecordAsPersonAsync(plugin, PersonId);
                return _mapper.Map<ManageModel.Plugin>(createdPlugin);
            }, nameof(GetPluginById), "pluginId");
        }

        [HttpGet("plugins")]
        public async Task<ActionResult<IEnumerable<ManageModel.Plugin>>> GetPlugins([FromQuery] string? search = null, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var allPlugins = await _repository.GetRecordsAsPersonAsync<Plugin>(PersonId, all);
                    return _mapper.Map<IEnumerable<ManageModel.Plugin>>(allPlugins);
                }

                var searchPlugins = await _repository.SearchRecordsAsPersonAsync<Plugin>(new[] { "Name", "Description" }, search, PersonId, all);
                return _mapper.Map<IEnumerable<ManageModel.Plugin>>(searchPlugins);
            });
        }

        [HttpGet("plugin/{pluginId}")]
        public async Task<ActionResult<ManageModel.Plugin>> GetPluginById(string pluginId)
        {
            return await HandleGet(async () =>
            {
                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId);
                return _mapper.Map<ManageModel.Plugin>(plugin);
            });
        }

        [HttpPut("plugin/{pluginId}")]
        public async Task<IActionResult> UpdatePlugin(string pluginId, [FromBody] Plugin plugin)
        {
            return await HandlePut(async () =>
            {
                if (plugin.Id == null)
                    throw new ArgumentNullException("Plugin Id is required.");

                if (plugin.Id != null && !plugin.Id.Equals(pluginId))
                {
                    throw new InvalidDataException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                plugin.Id = pluginId;

                await _repository.UpdateRecordAsPersonAsync(plugin, PersonId);
            });
        }

        [HttpDelete("plugin/{pluginId}")]
        public async Task<IActionResult> DeletePlugin(string pluginId)
        {
            return await HandleDelete(async () =>
            {
                await _repository.DeleteRecordAsPersonAsync<Plugin>(pluginId, PersonId);
            });
        }

        // *** PLUGIN_LIBRARY *** //
        /*

        [HttpPost("plugin/library/import/upload")]
        public async Task<ActionResult<string>> StartPluginLibraryUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded or file is empty.");
                }

                var tempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
                var zipPath = Path.Combine(tempDirectory, file.FileName);

                Directory.CreateDirectory(tempDirectory);

                // Save the uploaded file to a temporary location
                await using (var stream = new FileStream(zipPath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Extract the uploaded .zip file
                var extractDirectory = Path.Combine(tempDirectory, "extracted");
                System.IO.Compression.ZipFile.ExtractToDirectory(zipPath, extractDirectory);

                // Start the directory import process
                var importId = _pluginImportService.StartDirectoryImport(extractDirectory, (id, pluginLibrary) =>
                {
                    // Notify the caller via callback (caller is responsible for saving the plugin library)
                    _logger.LogInformation("Import completed for ID: {ImportId}", id);
                });

                return Ok(importId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to upload and start plugin library import.");
                return BadRequest(new { Error = ex.Message });
            }
        }



        [HttpPost("plugin/library/import/csproj")]
        public ActionResult<string> StartPluginLibraryRepoImport([FromQuery] string fileUrl)
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return BadRequest("File URL is required.");
                }

                var importId = _pluginImportService.StartRepoImport(fileUrl, (id, pluginLibrary) =>
                {
                    // Notify the caller via callback (caller is responsible for saving the plugin library)
                    _logger.LogInformation("Import completed for ID: {ImportId}", id);
                });

                return Ok(importId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start plugin library import.");
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpGet("plugin/library/import/{importId}/status")]
        public ActionResult<string> GetPluginLibraryImportStatus(string importId)
        {
            try
            {
                var status = _pluginImportService.GetImportStatus(importId);
                if (string.IsNullOrEmpty(status) || status == "NotFound")
                {
                    return NotFound("Import status not found.");
                }

                return Ok(status);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to retrieve import status for ID: {ImportId}", importId);
                return BadRequest(new { Error = ex.Message });
            }
        }


        [HttpGet("plugin/library/{pluginLibraryId}")]
        public async Task<ActionResult<PluginLibrary>> GetPluginLibraryFromImport(string pluginLibraryId)
        {
            var pluginLibrary = await _repository.GetRecordByIdAsPersonAsync<PluginLibrary>(pluginLibraryId, PersonId);
            if (pluginLibrary == null)
            {
                return NotFound("Plugin library not available or import not completed.");
            }

            return Ok(pluginLibrary);
        }
        */


        // *** PLUGIN_FUNCTION *** //


        [HttpPost("plugin/{pluginId}/function")]
        public async Task<ActionResult<ManageModel.Function>> CreateFunctionForPlugin(string pluginId, [FromBody] Function function)
        {
            return await HandlePost(async () =>
            {
                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId);

                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                function = await _repository.CreateRecordAsSystemAsync(function);

                var pluginFunction = await _repository.CreateRecordAsSystemAsync(new PluginFunction()
                {
                    PluginId = pluginId,
                    FunctionId = function.Id,
                    IsRoot = true
                });

                return _mapper.Map<ManageModel.Function>(function);

            }, nameof(FunctionController.GetFunctionById), "functionId", nameof(FunctionController));
        }

        [HttpGet("plugin/{pluginId}/functions")]
        public async Task<ActionResult<IEnumerable<ManageModel.Function>>> GetFunctionsForPlugin(string pluginId, [FromQuery] bool all)
        {
            return await HandleGet(async () =>
            {
                var functions = await _repository.GetChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", pluginId, PersonId, all);

                return _mapper.Map<IEnumerable<ManageModel.Function>>(functions);
            });
        }

        [HttpPost("plugin/{pluginId}/function/{functionId}")]
        public async Task<IActionResult> AddFunctionToPlugin(string pluginId, string functionId, [FromQuery] bool all)
        {
            return await HandlePut(async () =>
            {
                var function = await _repository.GetChildRecordByIdWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", functionId, PersonId, all);

                if (function == null)
                    throw new KeyNotFoundException("Function not found.");

                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId, all);

                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                var pluginFunctions = await _repository.QueryRecordsAsSystemAsync<PluginFunction>(new() { { "PluginId", pluginId }, { "FunctionId", functionId } });

                if (pluginFunctions.Count() != 0)
                    throw new InvalidOperationException("Function is already associated with Plugin.");

                await _repository.CreateRecordAsSystemAsync<PluginFunction>(new() { PluginId = pluginId, FunctionId = functionId });

            });
        }

        [HttpDelete("plugin/{pluginId}/function/{functionId}")]
        public async Task<IActionResult> RemoveFunctionFromPlugin(string pluginId, string functionId)
        {
            return await HandleDelete(async () =>
            {

                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId);

                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                var pluginFunctions = await _repository.QueryRecordsAsSystemAsync<PluginFunction>(new() { { "PluginId", pluginId }, { "FunctionId", functionId } });

                if (pluginFunctions.Count() == 0)
                    throw new InvalidOperationException("Function is not associated with Plugin.");

                await _repository.DeleteRecordAsSystemAsync<PluginFunction>(pluginFunctions.First().Id!);

            });
        }

        // *** FUNCTION_CONNECTION *** //

        [HttpPost("plugin/{pluginId}/functions/connection")]
        public async Task<ActionResult<ManageModel.FunctionConnection>> CreateFunctionConnectionForPlugin(string pluginId, [FromBody] FunctionConnection functionConnection, [FromQuery] bool all)
        {
            return await HandlePost(async () =>
            {
                if(functionConnection.FunctionId == null)
                {
                    throw new InvalidOperationException("FunctionId cannot be null.");
                }

                if (functionConnection.ConnectionId == null)
                {
                    throw new InvalidOperationException("ConnectionId cannot be null.");
                }

                // Validate plugin
                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId);
                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                // Validate connection
                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(functionConnection.ConnectionId, PersonId, all);
                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                // Fetch all functions associated with the plugin
                var functions = await _repository.GetChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>(
                    "PluginId", "FunctionId", pluginId, PersonId);

                if (!functions.Any(f => f.Id == functionConnection.FunctionId))
                    throw new InvalidOperationException("Function is not associated with this plugin.");

                functionConnection.Id = (await _repository.CreateRecordAsSystemAsync(functionConnection))?.Id ??
                    throw new InvalidOperationException("Could not create FunctionConnection.");

                return _mapper.Map<ManageModel.FunctionConnection>(functionConnection);
               
            },nameof(GetFunctionConnectionForPlugin), "functionConnectionId");
        }


        [HttpGet("plugin/{pluginId}/functions/connections")]
        public async Task<ActionResult<IEnumerable<ManageModel.FunctionConnection>>> GetFunctionConnectionsForPlugin(string pluginId, [FromQuery] bool all)
        {
            return await HandleGet(async () =>
            {
                // Fetch all functions associated with the plugin
                var functions = await _repository.GetChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>(
                    "PluginId", "FunctionId", pluginId, PersonId, all);

                if (!functions.Any())
                    return Enumerable.Empty<ManageModel.FunctionConnection>();

                var functionIds = functions.Select(f => f.Id);

                // Fetch connections for all the function IDs
                var connections = await _repository.GetChildRecordsByIdsAsSystemAsync<Function, FunctionConnection>(
                    "FunctionId", functionIds);

                var record = _mapper.Map<IEnumerable<ManageModel.FunctionConnection>>(connections);

                return record;
            });
        }

        [HttpGet("function/connection/{functionConnectionId}")]
        public async Task<ActionResult<ManageModel.FunctionConnection>> GetFunctionConnectionForPlugin(string functionConnectionId, [FromQuery] bool all)
        {
            throw new NotImplementedException();
        }


        [HttpDelete("plugin/{pluginId}/functions/connection/{connectionId}")]
        public async Task<IActionResult> RemoveConnectionFromPlugin(string pluginId, string connectionId)
        {
            return await HandleDelete(async () =>
            {
                // Validate plugin
                var plugin = await _repository.GetRecordByIdAsPersonAsync<Plugin>(pluginId, PersonId);
                if (plugin == null)
                    throw new KeyNotFoundException("Plugin not found.");

                // Fetch all functions associated with the plugin
                var functions = await _repository.GetChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>(
                    "PluginId", "FunctionId", pluginId, PersonId);

                if (!functions.Any())
                    throw new InvalidOperationException("No functions are associated with this plugin.");

                // Remove FunctionConnections for the specified connection
                foreach (var function in functions)
                {
                    var existingFunctionConnection = await _repository.QueryRecordsAsSystemAsync<FunctionConnection>(
                        new Dictionary<string, object>
                        {
                    { "FunctionId", function.Id },
                    { "ConnectionId", connectionId }
                        });

                    if (!existingFunctionConnection.Any())
                        continue;

                    await _repository.DeleteRecordAsSystemAsync<FunctionConnection>(existingFunctionConnection.First().Id);
                }
            });
        }


    }
}
