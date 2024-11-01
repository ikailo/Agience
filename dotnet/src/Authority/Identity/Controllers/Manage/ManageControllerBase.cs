using Agience.Authority.Identity.Data.Adapters;
using Agience.Authority.Identity.Exceptions;
using Agience.Core.Models.Entities;
using Microsoft.AspNetCore.Authentication.Google;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Agience.Authority.Identity.Controllers.Manage
{
    [Authorize(AuthenticationSchemes = $"Bearer,{GoogleDefaults.AuthenticationScheme}", Roles = "user", Policy = "manage")]
    public class ManageControllerBase : ControllerBase
    {
        protected readonly IAgienceDataAdapter _dataAdapter;
        protected readonly ILogger _logger;

        protected string PersonId { get { return HttpContext.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)?.Value ?? throw new HttpRequestException("No personId found."); } }

        public ManageControllerBase(IAgienceDataAdapter dataAdapter, ILogger logger)
        {
            _dataAdapter = dataAdapter;
            _logger = logger;
        }

        protected async Task<ActionResult> HandleRequest(Func<Task<ActionResult>> action)
        {
            try
            {
                return await action();
            }
            catch (NotFoundException)
            {
                return NotFound();
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

        protected async Task<ActionResult> HandleGet<T>(Func<Task<T>> action)
        {
            return await HandleRequest(async () =>
            {
                var result = await action();

                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            });
        }

        protected async Task<IActionResult> HandleRedirect(Func<Task<string?>> action)
        {
            return await HandleRequest(async () =>
            {
                var redirectUrl = await action();

                if (string.IsNullOrEmpty(redirectUrl))
                {
                    return NotFound();
                }

                return Redirect(redirectUrl);
            });
        }

        protected async Task<ActionResult> HandlePost<T>(Func<Task<T?>> action, string getActionName) where T : AgienceEntity
        {
            return await HandleRequest(async () =>
            {
                var result = await action();

                if (result == null)
                {
                    return NotFound();
                }

                return CreatedAtAction(getActionName, result , result);
            });
        }

        private async Task<IActionResult> HandlePutOrDelete(Func<Task> action)
        {
            return await HandleRequest(async () =>
            {
                await action();

                return new NoContentResult();
            });
        }

        protected async Task<IActionResult> HandlePut(Func<Task> action)
        {
            return await HandlePutOrDelete(action);
        }

        protected async Task<IActionResult> HandleDelete(Func<Task> action)
        {
            return await HandlePutOrDelete(action);
        }
    }
}