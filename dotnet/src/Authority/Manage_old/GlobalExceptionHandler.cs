using Microsoft.AspNetCore.Diagnostics;

namespace Agience.Authority.Manage
{
    internal sealed class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(
                exception, "Unhandled Exception occurred: {Message}", exception.Message);

            httpContext.Response.Redirect(String.Format("/Error"));

            return true;
        }
    }
}