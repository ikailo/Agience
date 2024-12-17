using Microsoft.AspNetCore.Mvc;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class PersonController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public PersonController(RecordsRepository repository, ILogger<PersonController> logger, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }
                
        [HttpGet("person")]
        public async Task<ActionResult<ManageModel.Person>> GetPerson()
        {
            return await HandleGet(async () =>
            {
                var personEntity = await _repository.GetRecordByIdAsSystemAsync<Person>(PersonId); // This is the logged in user

                if (personEntity == null)
                {
                    throw new KeyNotFoundException("Person not found."); // This should never happen
                }

                return _mapper.Map<ManageModel.Person>(personEntity);
            });
        }
                
        [HttpPut("person/{personId}")]
        public async Task<IActionResult> UpdatePerson(string personId, [FromBody] Person person)
        {
            return await HandlePut(async () =>
            {
                if (person.Id != PersonId)
                {
                    throw new InvalidOperationException("Person not found.");
                }
                
                if (person?.Id == null)
                    throw new ArgumentNullException("Person Id is required.");

                if (person.Id != null && !person.Id.Equals(personId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                person.Id = personId;

                person = await _repository.UpdateRecordAsSystemAsync(person);
                
            });
        }
    }
}
