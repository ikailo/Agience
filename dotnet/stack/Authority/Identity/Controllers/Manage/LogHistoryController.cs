using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Models.Manage;
using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Route("manage")]
    [ApiController]
    public class LogHistoryController : ManageControllerBase
    {
        private readonly IMapper _mapper;
        public LogHistoryController(IAgienceDataAdapter dataAdapter, ILogger<AgencyController> logger, IMapper mapper)
            : base(dataAdapter, logger)
        {
            _mapper = mapper;
        }

        /// <summary>
        /// This method is to get the logs by agent id
        /// </summary>
        /// <param name="id"></param>
        /// <returns>List of log</returns>
        [HttpGet("log/{id}")]
        public async Task<ActionResult<IEnumerable<Log>>> GetLog(string id)
        {
            try
            {
                return await HandleGet(async () =>
                {
                    var log = await _dataAdapter.GetRecordsAsPersonAsync<Models.Log>(id);
                    return _mapper.Map<IEnumerable<Log>>(log);
                });
            }
            catch (Exception ex) {
                _logger.Log(LogLevel.Error, "Failed to get log!");
                _logger.Log(LogLevel.Error, ex?.Message);
                return BadRequest(ex?.InnerException?.Message);
            }
        }

        /// <summary>
        /// This method is to add the log into table log
        /// </summary>
        /// <param name="log"></param>
        /// <returns>Return the newly created log object</returns>
        [HttpPost("log")]
        public async Task<ActionResult> PostLog([FromBody] Log log)
        {
            try
            {
                if (log == null)
                {
                    return BadRequest("Request body cannot be null.");
                }
                return await HandlePost(async () =>
                {
                    var modelLog = _mapper.Map<Models.Log>(log);
                    return await _dataAdapter.CreateRecordAsPersonAsync(modelLog, PersonId);
                }, nameof(GetLog));

            }
            catch(Exception ex)
            {
                _logger.Log(LogLevel.Error, "Failed to add log!");
                _logger.Log(LogLevel.Error, ex?.Message);
                return BadRequest(ex?.InnerException?.Message);
            }
        }
        
    }
}
