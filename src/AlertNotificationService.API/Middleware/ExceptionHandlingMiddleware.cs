using AlertNotificationService.Domain.Exceptions;
using System.Text.Json;

namespace AlertNotificationService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, title, errors) = exception switch
        {
            NotFoundException => (StatusCodes.Status404NotFound, "Not Found", (object?)null),
            Domain.Exceptions.ValidationException ve => (
                StatusCodes.Status400BadRequest,
                "Validation Error",
                (object)ve.Errors),
            DomainException => (StatusCodes.Status400BadRequest, "Domain Error", (object?)null),
            _ => (StatusCodes.Status500InternalServerError, "Internal Server Error", (object?)null)
        };

        context.Response.StatusCode = statusCode;

        var response = new
        {
            status = statusCode,
            title,
            detail = exception.Message,
            errors
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
