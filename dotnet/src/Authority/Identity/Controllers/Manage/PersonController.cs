using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Agience.Authority.Identity.Services;
using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using AutoMapper;
using Microsoft.AspNetCore.Identity;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class PersonController : ManageControllerBase
    {
        private readonly IMapper _mapper;

        public PersonController(IAgienceDataAdapter dataAdapter, ILogger<AgentController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        [HttpGet("person")]
        public async Task<ActionResult<Person>> GetPerson()
        {
            return await HandleGet(async () =>
            {
                var modelPerson = await _dataAdapter.GetRecordByIdAsPersonAsync<Models.Person>(PersonId, PersonId);
                return _mapper.Map<Person>(modelPerson);
            }
            );
        }

        [HttpPut("person")]
        public async Task<IActionResult> PutPerson([FromBody] Person person)
        {
            return await HandlePut(async () =>
            {
                var modelPerson = _mapper.Map<Models.Person>(person);
                await _dataAdapter.UpdateRecordAsPersonAsync(modelPerson, PersonId);
            });
        }
    }
}
