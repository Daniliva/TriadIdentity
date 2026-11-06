using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Diagnostics;

namespace TriadIdentity.UI.Api.Middleware
{
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async  ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {

            var (status, title) = exception switch
            {
                ValidationException => (StatusCodes.Status400BadRequest, "Validation error"),
                _ => (StatusCodes.Status500InternalServerError, "Internal server error")
            };
            if (status >= 500)
            {
                _logger.LogError(exception, "Unhandled exception on {Path}", httpContext.Request.Path);
            }

            httpContext.Response.StatusCode = status;
            await httpContext.Response.WriteAsJsonAsync(new
            {
                Status = status,
                Title = title,
                Detail = status <500? exception.Message : "An unexpected error occurred.",
                Instance = httpContext.Request.Path
            }, cancellationToken);

            return true;
        }
    }
}
