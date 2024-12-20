using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Agience.Authority.Identity.Data.Repositories;
using Agience.Authority.Identity.Models;
using ManageModel = Agience.Authority.Models.Manage;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class TopicController : ManageControllerBase
    {
        private readonly RecordsRepository _repository;
        private readonly IMapper _mapper;

        public TopicController(ILogger<TopicController> logger, RecordsRepository repository, IMapper mapper)
            : base(logger)
        {
            _repository = repository;
            _mapper = mapper;
        }

        // *** TOPIC *** //

        [HttpPost("topic")]
        public async Task<ActionResult<ManageModel.Topic>> CreateTopic([FromBody] Topic topic)
        {
            return await HandlePost(async () =>
            {
                topic = await _repository.CreateRecordAsPersonAsync(topic, PersonId);

                return _mapper.Map<ManageModel.Topic>(topic);
            }, nameof(GetTopicById), "topicId");
        }


        [HttpGet("topics")]
        public async Task<ActionResult<IEnumerable<ManageModel.Topic>>> GetTopics([FromQuery] string? search = null)
        {
            return await HandleGet(async () =>
            {
                if (search == null)
                {
                    var topics = await _repository.GetRecordsAsPersonAsync<Topic>(PersonId);
                    return _mapper.Map<IEnumerable<ManageModel.Topic>>(topics);
                }

                var searchResults = await _repository.SearchRecordsAsPersonAsync<Topic>(
                    new[] { "Name", "Description" }, search, PersonId);

                return _mapper.Map<IEnumerable<ManageModel.Topic>>(searchResults);
            });
        }

        [HttpGet("topic/{topicId}")]
        public async Task<ActionResult<ManageModel.Topic>> GetTopicById(string topicId)
        {
            return await HandleGet(async () =>
            {
                var topic = await _repository.GetRecordByIdAsPersonAsync<Topic>(topicId, PersonId);

                if (topic == null)
                    throw new KeyNotFoundException("Topic not found.");

                return _mapper.Map<ManageModel.Topic>(topic);
            });
        }

        [HttpPut("topic/{topicId}")]
        public async Task<IActionResult> UpdateTopic(string topicId, [FromBody] Topic topic)
        {
            return await HandlePut(async () =>
            {
                if (topic?.Id == null)
                    throw new ArgumentNullException("Topic Id is required.");

                if (topic.Id != null && !topic.Id.Equals(topicId))
                {
                    throw new InvalidOperationException("If an Id is provided in the body, it must match the Id in the URL.");
                }

                topic.Id = topicId;

                await _repository.UpdateRecordAsPersonAsync(topic, PersonId);
            });
        }

        [HttpDelete("topic/{topicId}")]
        public async Task<IActionResult> DeleteTopic(string topicId)
        {
            return await HandleDelete(async () =>
            {
                var success = await _repository.DeleteRecordAsPersonAsync<Topic>(topicId, PersonId);

                if (!success)
                    throw new KeyNotFoundException("Topic not found or could not be deleted.");
            });
        }

        // *** AGENTS *** //

        [HttpGet("topic/{topicId}/agents")]
        public async Task<ActionResult<IEnumerable<ManageModel.Agent>>> GetAgentsForTopic(string topicId, [FromQuery] bool all)
        {
            return await HandleGet(async () =>
            {
                var agents = await _repository.GetChildRecordsWithJoinAsPersonAsync<Topic, Agent, AgentTopic>("TopicId", "AgentId", topicId, PersonId, all);

                return _mapper.Map<IEnumerable<ManageModel.Agent>>(agents);
            });
        }

        [HttpPost("topic/{topicId}/agent/{agentId}")]
        public async Task<IActionResult> AddAgentToTopic(string topicId, string agentId, [FromQuery] bool all)
        {
            return await HandlePut(async () =>
            {
                var agent = await _repository.GetRecordByIdAsPersonAsync<Agent>(agentId, PersonId);

                if (agent == null)
                    throw new KeyNotFoundException("Agent not found.");

                var topic = await _repository.GetRecordByIdAsPersonAsync<Topic>(topicId, PersonId, all);

                if (topic == null)
                    throw new KeyNotFoundException("Topic not found.");

                var topicAgents = await _repository.QueryRecordsAsSystemAsync<AgentTopic>(new() { 
                    { "TopicId", topicId }, 
                    { "AgentId", agentId } 
                });

                if (topicAgents.Count() != 0)
                    throw new InvalidOperationException("Agent is already associated with Topic.");

                await _repository.CreateRecordAsSystemAsync<AgentTopic>(new() { TopicId = topicId, AgentId = agentId });
            });
        }

        [HttpDelete("topic/{topicId}/agent/{agentId}")]
        public async Task<IActionResult> RemoveAgentFromTopic(string topicId, string agentId)
        {
            return await HandleDelete(async () =>
            {

                var topic = await _repository.GetRecordByIdAsPersonAsync<Topic>(topicId, PersonId);

                if (topic == null)
                    throw new KeyNotFoundException("Topic not found.");

                var topicAgents = await _repository.QueryRecordsAsSystemAsync<AgentTopic>(new() { { "TopicId", topicId }, { "AgentId", agentId } });

                if (topicAgents.Count() == 0)
                    throw new InvalidOperationException("Agent is not associated with Topic.");

                await _repository.DeleteRecordAsSystemAsync<AgentTopic>(topicAgents.First().Id!);

            });
        }

    }
}
