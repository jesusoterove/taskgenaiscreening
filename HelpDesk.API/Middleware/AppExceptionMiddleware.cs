using System.Text.Json;
using HelpDesk.Application.Common;

namespace HelpDesk.API.Middleware;

public sealed class AppExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<AppExceptionMiddleware> _logger;

    public AppExceptionMiddleware(RequestDelegate next, ILogger<AppExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (AppException ex)
        {
            context.Response.StatusCode = ex.StatusCode;
            context.Response.ContentType = "application/problem+json";
            var payload = new
            {
                type = "about:blank",
                title = ex.Message,
                status = ex.StatusCode
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(payload));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled error");
            context.Response.StatusCode = StatusCodes.Status500InternalServerError;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsync("{\"type\":\"about:blank\",\"title\":\"Unexpected error\",\"status\":500}");
        }
    }
}
