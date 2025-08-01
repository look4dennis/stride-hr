using System.Net;
using System.Text.Json;

namespace StrideHR.API.Middleware;

public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
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
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = new ApiErrorResponse
        {
            Success = false,
            Message = GetErrorMessage(exception),
            Errors = GetErrorDetails(exception),
            TraceId = context.TraceIdentifier
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = GetStatusCode(exception);

        var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(jsonResponse);
    }

    private static string GetErrorMessage(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => "Unauthorized access",
            ArgumentException => "Invalid argument provided",
            KeyNotFoundException => "Resource not found",
            InvalidOperationException => "Invalid operation",
            _ => "An error occurred while processing your request"
        };
    }

    private static List<string> GetErrorDetails(Exception exception)
    {
        var errors = new List<string>();

        if (exception is AggregateException aggregateException)
        {
            foreach (var innerException in aggregateException.InnerExceptions)
            {
                errors.Add(innerException.Message);
            }
        }
        else
        {
            errors.Add(exception.Message);
        }

        return errors;
    }

    private static int GetStatusCode(Exception exception)
    {
        return exception switch
        {
            UnauthorizedAccessException => (int)HttpStatusCode.Unauthorized,
            ArgumentException => (int)HttpStatusCode.BadRequest,
            KeyNotFoundException => (int)HttpStatusCode.NotFound,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };
    }
}

public class ApiErrorResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public List<string> Errors { get; set; } = new();
    public string? TraceId { get; set; }
}