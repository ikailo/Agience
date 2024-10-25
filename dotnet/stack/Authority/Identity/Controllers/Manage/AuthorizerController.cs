using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public partial class AuthorizerController : ManageControllerBase
    {
        private readonly IMapper _mapper;

        public AuthorizerController(IAgienceDataAdapter dataAdapter, ILogger<CallbackController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        [HttpGet("authorizers")]
        public async Task<ActionResult<IEnumerable<Authorizer>>> GetAuthorizers()
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<IEnumerable<Authorizer>>(await _dataAdapter.GetRecordsAsPersonAsync<Models.Authorizer>(PersonId));
            });
        }

        [HttpGet("authorizer/{id}")]
        public async Task<ActionResult<Authorizer>> GetAuthorizer(string id)
        {
            return await HandleGet(async () =>
            {
                return _mapper.Map<Authorizer>(await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Authorizer>(id, PersonId));
            });
        }

        [HttpPost("authorizer")]
        public async Task<ActionResult> PostAuthorizer([FromBody] Authorizer authorizer)
        {
            return await HandlePost(async () =>
            {
                return await _dataAdapter.CreateRecordAsPersonAsync(_mapper.Map<Models.Authorizer>(authorizer), PersonId);
            },
                nameof(GetAuthorizer)
            );
        }

        [HttpPut("authorizer")]
        public async Task<IActionResult> PutAuthorizer([FromBody] Authorizer authorizer)
        {
            return await HandlePut(async () =>
            {
                await _dataAdapter.UpdateRecordAsPersonAsync(_mapper.Map<Models.Authorizer>(authorizer), PersonId);
            });
        }

        [HttpDelete("authorizer/{id}")]
        public async Task<IActionResult> DeleteAuthorizer(string id)
        {
            return await HandleDelete(async () =>
            {
                await _dataAdapter.DeleteRecordAsPersonAsync<Models.Authorizer>(id, PersonId);
            });
        }
         
        [HttpGet("authorizer/{id}/activate")]
        public async Task<IActionResult> GetAuthorizerActivate(string id, [FromQuery] string state)
        {
            throw new NotImplementedException();
        }
    }
}
