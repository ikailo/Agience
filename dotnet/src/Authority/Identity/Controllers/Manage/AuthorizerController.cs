using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class AuthorizerController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public AuthorizerController(ILogger<AuthorizerController> logger, RecordsRepository repository, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // Create a new authorizer
        [HttpPost("authorizer")]
        public async Task<ActionResult<ManageModel.Authorizer>> CreateAuthorizer([FromBody] Authorizer authorizer)
        {
            return await HandlePost(async () =>
            {   
                authorizer = await _repository.CreateRecordAsPersonAsync(authorizer, PersonId);

                return _mapper.Map<ManageModel.Authorizer>(authorizer);
            }, nameof(GetAuthorizerById), "authorizerId");
        }

        // Retrieve a list of all authorizers
        [HttpGet("authorizers")]
        public async Task<ActionResult<IEnumerable<ManageModel.Authorizer>>> GetAuthorizers([FromQuery] string? search = null)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var authorizers = await _repository.GetRecordsAsPersonAsync<Authorizer>(PersonId);
                    return _mapper.Map<IEnumerable<ManageModel.Authorizer>>(authorizers);
                }
                
                var searchResults = await _repository.SearchRecordsAsPersonAsync<Authorizer>(
                    new[] { "Name", "Description" }, search, PersonId);

                return _mapper.Map<IEnumerable<ManageModel.Authorizer>>(searchResults);
            });
        }

        [HttpGet("authorizer/{authorizerId}")]
        public async Task<ActionResult<ManageModel.Authorizer>> GetAuthorizerById(string authorizerId)
        {
            return await HandleGet(async () =>
            {
                var authorizer = await _repository.GetRecordByIdAsPersonAsync<Authorizer>(authorizerId, PersonId);

                if (authorizer == null)
                    throw new KeyNotFoundException("Authorizer not found.");

                return _mapper.Map<ManageModel.Authorizer>(authorizer);
            });
        }

        [HttpPut("authorizer/{authorizerId}")]
        public async Task<IActionResult> UpdateAuthorizer(string authorizerId, [FromBody] Authorizer authorizer)
        {
            return await HandlePut(async () =>
            {
                if (authorizer?.Id == null)
                    throw new ArgumentNullException("Authorizer Id is required.");

                if (authorizer.Id != null && !authorizer.Id.Equals(authorizerId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                authorizer.Id = authorizerId;

                await _repository.UpdateRecordAsPersonAsync(authorizer, PersonId);
            });
        }

        [HttpDelete("authorizer/{authorizerId}")]
        public async Task<IActionResult> DeleteAuthorizer(string authorizerId)
        {
            return await HandleDelete(async () =>
            {
                var success = await _repository.DeleteRecordAsPersonAsync<Authorizer>(authorizerId, PersonId);

                if (!success)
                    throw new KeyNotFoundException("Authorizer not found or could not be deleted.");
            });
        }

        // *** CONNECTIONS *** //

        [HttpGet("authorizer/{authorizerId}/connections")]
        public async Task<ActionResult<IEnumerable<ManageModel.Connection>>> GetConnectionsForAuthorizer(string authorizerId, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                var connections = await _repository.GetChildRecordsWithJoinAsPersonAsync<Authorizer, Connection, ConnectionAuthorizer>("AuthorizerId", "ConnectionId", authorizerId, PersonId, all);
                return _mapper.Map<IEnumerable<ManageModel.Connection>>(connections);
            });
        }


        [HttpPost("authorizer/{authorizerId}/connection/{connectionId}")]
        public async Task<IActionResult> AddConnectionToAuthorizer( string authorizerId, string connectionId, [FromQuery] bool all = false)
        {
            return await HandleLink(async () =>
            {
                var authorizer = await _repository.GetRecordByIdAsPersonAsync<Authorizer>(authorizerId, PersonId);

                if (authorizer == null)
                    throw new KeyNotFoundException("Authorizer not found.");

                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(connectionId, PersonId, all);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                await _repository.CreateRecordAsSystemAsync(new ConnectionAuthorizer()
                {                    
                    AuthorizerId = authorizerId,
                    ConnectionId = connectionId
                });
            });
        }

        [HttpDelete("authorizer/{authorizerId}/connection/{connectionId}")]
        public async Task<IActionResult> RemoveConnectionFromAuthorizer(string authorizerId, string connectionId, [FromQuery] bool all = false)
        {
            return await HandleDelete(async () =>
            {
                var authorizer = await _repository.GetRecordByIdAsPersonAsync<Authorizer>(authorizerId, PersonId);

                if (authorizer == null)
                    throw new KeyNotFoundException("Authorizer not found.");

                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(connectionId, PersonId, all);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                var connectionAuthorizers = await _repository.QueryRecordsAsSystemAsync<ConnectionAuthorizer>(new()                {                    
                    {"AuthorizerId", authorizerId },
                    {"ConnectionId", connectionId }
                });

                if (connectionAuthorizers.Count() != 1)
                    throw new InvalidDataException("ConnectionAuthorizer not found.");

                await _repository.DeleteRecordAsSystemAsync<ConnectionAuthorizer>(connectionAuthorizers.First().Id!);
            });
        }
    }
}
