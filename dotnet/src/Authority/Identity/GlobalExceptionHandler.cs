using System.Text.Json;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace Agience.Authority.Identity;

internal sealed class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger, IWebHostEnvironment environment)
    {
        _logger = logger;
        _environment = environment;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        // Log the exception
        _logger.LogError(exception, "Unhandled Exception occurred: {Message}", exception.Message);

        // Set the response status code and content type
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;
        httpContext.Response.ContentType = "application/json";

        // Create an error response object
        var errorResponse = new
        {
            message = new[] {"development","debug","local"}.Contains(_environment.EnvironmentName, StringComparer.OrdinalIgnoreCase)
                ? exception.Message // Show detailed error in development
                : "An unexpected error occurred." // Generic error message in production
        };

        // Serialize the error response to JSON
        var errorJson = JsonSerializer.Serialize(errorResponse);

        // Write the error response to the HTTP response body
        await httpContext.Response.WriteAsync(errorJson, cancellationToken);

        return true; // Indicate that the exception has been handled
    }
}
