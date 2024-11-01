using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Services;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class AgencyController : ManageControllerBase
    {
        private readonly IMapper _mapper;

        public AgencyController(IAgienceDataAdapter dataAdapter, ILogger<AgencyController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        [HttpGet("agencies")]
        public async Task<ActionResult<IEnumerable<Agency>>> GetAgencies()
        {
            return await HandleGet(async () =>
            {
                var agencies = await _dataAdapter.GetRecordsAsPersonAsync<Models.Agency>(PersonId);
                return _mapper.Map<IEnumerable<Agency>>(agencies);
            });
        }

        [HttpGet("agency/{id}")]
        public async Task<ActionResult<Agency>> GetAgency(string id)
        {
            return await HandleGet(async () =>
            {
                var agency = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Agency>(id, PersonId);
                return _mapper.Map<Agency>(agency);
            });
        }

        [HttpPost("agency")]
        public async Task<ActionResult> PostAgency([FromBody] Agency agency)
        {
            return await HandlePost(async () =>
            {
                var modelAgency = _mapper.Map<Models.Agency>(agency);
                return await _dataAdapter.CreateRecordAsPersonAsync(modelAgency, PersonId);                
            }, nameof(GetAgency));
        }

        [HttpPut("agency")]
        public async Task<IActionResult> PutAgency([FromBody] Agency agency)
        {
            return await HandlePut(async () =>
            {
                var modelAgency = _mapper.Map<Models.Agency>(agency);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelAgency, PersonId);
            });
        }

        [HttpDelete("agency/{id}")]
        public async Task<IActionResult> DeleteAgency(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Agency>(id, PersonId);
            });
        }
    }
}
