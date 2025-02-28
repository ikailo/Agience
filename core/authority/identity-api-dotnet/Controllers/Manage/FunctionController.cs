using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class FunctionController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public FunctionController(ILogger<FunctionController> logger, RecordsRepository repository, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        [HttpGet("functions")]
        public async Task<ActionResult<IEnumerable<ManageModel.Function>>> GetFunctions([FromQuery] string? search = null, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var allFunctions = await _repository.GetChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", PersonId, all);
                    return _mapper.Map<IEnumerable<ManageModel.Function>>(allFunctions);
                }

                // Search functions associated with accessible plugins
                var functions = await _repository.SearchChildRecordsWithJoinAsPersonAsync<Plugin, Function, PluginFunction>(
                    parentForeignKey: "PluginId",
                    childForeignKey: "FunctionId",
                    personId: PersonId,
                    searchTerm: search,
                    searchFields: new[] { "Name", "Description" },
                    includePublic: all
                );

                return _mapper.Map<IEnumerable<ManageModel.Function>>(functions);
            });
        }

        [HttpGet("function/{functionId}")]
        public async Task<ActionResult<ManageModel.Function>> GetFunctionById(string functionId, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                var function = await _repository.GetChildRecordByIdWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", functionId, PersonId, all);

                if (function == null)
                {
                    throw new KeyNotFoundException("Function not found.");
                }
                return _mapper.Map<ManageModel.Function>(function);
            });
        }


        [HttpPut("function/{functionId}")]
        public async Task<IActionResult> UpdateFunction(string functionId, [FromBody] Function function)
        {
            return await HandlePut(async () =>
            {
                if (function?.Id == null)
                    throw new ArgumentNullException("Connection Id is required.");

                if (function.Id != null && !function.Id.Equals(functionId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                function = await _repository.GetChildRecordByIdWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", functionId, PersonId) ??
                    throw new KeyNotFoundException("Function not found.");

                function.Id = functionId;

                await _repository.UpdateRecordAsSystemAsync(function);
            });
        }

        [HttpDelete("function/{functionId}")]
        public async Task<IActionResult> DeleteFunction(string functionId)
        {
            return await HandleDelete(async () =>
            {
                var function = await _repository.GetChildRecordByIdWithJoinAsPersonAsync<Plugin, Function, PluginFunction>("PluginId", "FunctionId", functionId, PersonId);

                if (function == null)
                {
                    throw new KeyNotFoundException("Function not found.");
                }

                var success = await _repository.DeleteRecordAsSystemAsync<Function>(functionId);
                if (!success)
                {
                    throw new KeyNotFoundException("Function not found or could not be deleted.");
                }
            });
        }
    }
}
