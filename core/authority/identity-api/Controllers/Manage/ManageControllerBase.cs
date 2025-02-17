using Agience.Core.Models.Entities.Abstract;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Authorize(AuthenticationSchemes = $"Bearer,{GoogleDefaults.AuthenticationScheme}", Roles = "user", Policy = "manage")]
    public abstract class ManageControllerBase : ControllerBase
    {
        protected readonly ILogger _logger;

        protected string PersonId
        {
            get
            {
                return HttpContext.User.Claims
                    .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value
                    ?? throw new HttpRequestException("No personId found.");
            }
        }

        protected ManageControllerBase(ILogger logger)
        {
            _logger = logger;
        }

        private async Task<ActionResult<T>> HandleRequest<T>(Func<Task<ActionResult<T>>> action)
        {
            try
            {
                return await action();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Not Found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Bad Request: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Server Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }

        protected async Task<ActionResult<T>> HandleGet<T>(Func<Task<T?>> action) where T : class
        {
            return await HandleRequest<T>(async () =>
            {
                var result = await action();

                if (result == null)
                {
                    return NotFound();
                }

                return Ok(result);
            });
        }

        protected async Task<ActionResult<IEnumerable<T>>> HandleGet<T>(Func<Task<IEnumerable<T>?>> action)
        {
            return await HandleRequest<IEnumerable<T>>(async () =>
            {
                var result = await action();

                return Ok(result);
            });
        }

        protected async Task<ActionResult<T>> HandlePost<T>(
            Func<Task<T?>> action,
            string getActionName,
            string getActionIdParameterName = "id",
            string? getActionControllerName = null
            
        ) where T : BaseEntity
        {
            return await HandleRequest<T>(async () =>
            {
                var result = await action();

                if (result?.Id == null)
                {
                    return NotFound();
                }

                var routeValues = new Dictionary<string, object>
                {
                    { getActionIdParameterName, result.Id }
                };

                if (getActionControllerName == null)
                {
                    return CreatedAtAction(getActionName, routeValues, result);
                }
                else
                {
                    return CreatedAtAction(getActionName, getActionControllerName.Replace("Controller", ""), routeValues, result);
                }
            });
        }

        private async Task<IActionResult> HandlePutOrDeleteOrLink(Func<Task> action)
        {
            try
            {
                await action();
                return NoContent();
            }
            catch (KeyNotFoundException ex)
            {
                _logger.LogError(ex, $"Not Found: {ex.Message}");
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, $"Bad Request: {ex.Message}");
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Server Error: {ex.Message}");
                return StatusCode(StatusCodes.Status500InternalServerError, "An unexpected error occurred.");
            }
        }


        protected async Task<IActionResult> HandlePut(Func<Task> action)
        {
            return await HandlePutOrDeleteOrLink(action);
        }

        protected async Task<IActionResult> HandleDelete(Func<Task> action)
        {
            return await HandlePutOrDeleteOrLink(action);
        }

        protected async Task<IActionResult> HandleLink(Func<Task> action)
        {
            return await HandlePutOrDeleteOrLink(action);
        }
    }
}
