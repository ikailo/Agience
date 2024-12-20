using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;
using NuGet.Protocol.Core.Types;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class ConnectionController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public ConnectionController(ILogger<ConnectionController> logger, RecordsRepository repository, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // Create a new connection
        [HttpPost("connection")]
        public async Task<ActionResult<ManageModel.Connection>> CreateConnection([FromBody] Connection connection)
        {
            return await HandlePost(async () =>
            {   
                connection = await _repository.CreateRecordAsPersonAsync(connection, PersonId);

                return _mapper.Map<ManageModel.Connection>(connection);
            }, nameof(GetConnectionById), "connectionId");
        }

        // Retrieve a list of all connections
        [HttpGet("connections")]
        public async Task<ActionResult<IEnumerable<ManageModel.Connection>>> GetConnections([FromQuery] string? search = null)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var connections = await _repository.GetRecordsAsPersonAsync<Connection>(PersonId);
                    return _mapper.Map<IEnumerable<ManageModel.Connection>>(connections);
                }
                
                var searchResults = await _repository.SearchRecordsAsPersonAsync<Connection>(
                    new[] { "Name", "Description" }, search, PersonId);

                return _mapper.Map<IEnumerable<ManageModel.Connection>>(searchResults);
            });
        }

        [HttpGet("connection/{connectionId}")]
        public async Task<ActionResult<ManageModel.Connection>> GetConnectionById(string connectionId)
        {
            return await HandleGet(async () =>
            {
                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(connectionId, PersonId);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                return _mapper.Map<ManageModel.Connection>(connection);
            });
        }

        [HttpPut("connection/{connectionId}")]
        public async Task<IActionResult> UpdateConnection(string connectionId, [FromBody] Connection connection)
        {
            return await HandlePut(async () =>
            {
                if (connection?.Id == null)
                    throw new ArgumentNullException("Connection Id is required.");

                if (connection.Id != null && !connection.Id.Equals(connectionId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                connection.Id = connectionId;

                await _repository.UpdateRecordAsPersonAsync(connection, PersonId);
            });
        }

        [HttpDelete("connection/{connectionId}")]
        public async Task<IActionResult> DeleteConnection(string connectionId)
        {
            return await HandleDelete(async () =>
            {
                var success = await _repository.DeleteRecordAsPersonAsync<Connection>(connectionId, PersonId);

                if (!success)
                    throw new KeyNotFoundException("Connection not found or could not be deleted.");
            });
        }

        // *** AUTHORIZER *** //

        [HttpGet("connection/{connectionId}/authorizers")]
        public async Task<ActionResult<IEnumerable<ManageModel.Authorizer>>> GetAuthorizersForConnection(string connectionId, [FromQuery] bool all = false)
        {
            return await HandleGet(async () =>
            {
                var authorizers = await _repository.GetChildRecordsWithJoinAsPersonAsync<Connection, Authorizer, ConnectionAuthorizer>("ConnectionId", "AuthorizerId", connectionId, PersonId, all);
                return _mapper.Map<IEnumerable<ManageModel.Authorizer>>(authorizers);
            });
        }

        
        [HttpPost("connection/{connectionId}/authorizer/{authorizerId}")]
        public async Task<IActionResult> AddAuthorizerToConnection(string connectionId, string authorizerId, [FromQuery] bool all = false)
        {
            return await HandleLink(async () =>
            {
                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(connectionId, PersonId);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                var authorizer = await _repository.GetRecordByIdAsPersonAsync<Authorizer>(authorizerId, PersonId, all);

                if (authorizer == null)
                    throw new KeyNotFoundException("Authorizer not found.");

                await _repository.CreateRecordAsSystemAsync(new ConnectionAuthorizer()
                {
                    ConnectionId = connectionId,
                    AuthorizerId = authorizerId
                });
            });
        }

        [HttpDelete("connection/{connectionId}/authorizer/{authorizerId}")]
        public async Task<IActionResult> RemoveAuthorizerFromConnection(string connectionId, string authorizerId, [FromQuery] bool all = false)
        {
            return await HandleDelete(async () =>
            {
                var connection = await _repository.GetRecordByIdAsPersonAsync<Connection>(connectionId, PersonId);

                if (connection == null)
                    throw new KeyNotFoundException("Connection not found.");

                var authorizer = await _repository.GetRecordByIdAsPersonAsync<Authorizer>(authorizerId, PersonId, all);

                if (authorizer == null)
                    throw new KeyNotFoundException("Authorizer not found.");

                var connectionAuthorizers = await _repository.QueryRecordsAsSystemAsync<ConnectionAuthorizer>(new()                {
                    {"ConnectionId", connectionId },
                    {"AuthorizerId", authorizerId },
                });

                if (connectionAuthorizers.Count() != 1)
                    throw new InvalidDataException("ConnectionAuthorizer not found.");

                await _repository.DeleteRecordAsSystemAsync<ConnectionAuthorizer>(connectionAuthorizers.First().Id!);
            });
        }
    }
}
