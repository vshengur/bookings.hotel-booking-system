using FluentValidation;

using Microsoft.AspNetCore.Mvc;

using PaymentService.API.Services;

namespace PaymentService.API.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
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
        catch (ValidationException ex)
        {
            _logger.LogWarning(ex, "Validation failed for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ValidationProblemDetails(
                ex.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(
                        group => group.Key,
                        group => group.Select(error => error.ErrorMessage).ToArray()))
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation failed",
                Detail = "One or more validation errors occurred.",
                Instance = $"{context.Request.Method} {context.Request.Path}"
            });
        }
        catch (PaymentProviderUnavailableException ex)
        {
            _logger.LogError(ex, "Payment provider unavailable for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            context.Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
            context.Response.ContentType = "application/problem+json";

            await context.Response.WriteAsJsonAsync(new ProblemDetails
            {
                Status = StatusCodes.Status503ServiceUnavailable,
                Title = "Payment provider unavailable",
                Detail = ex.Message,
                Instance = $"{context.Request.Method} {context.Request.Path}",
                Extensions =
                {
                    ["retryAfterSeconds"] = ex.RetryAfterSeconds
                }
            });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Resource not found for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status404NotFound,
                "Resource not found",
                ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled payment exception for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status500InternalServerError,
                "Internal server error",
                "An unexpected error occurred.");
        }
    }

    private static Task WriteProblemDetailsAsync(
        HttpContext context,
        int statusCode,
        string title,
        string detail)
    {
        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/problem+json";

        return context.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = $"{context.Request.Method} {context.Request.Path}"
        });
    }
}
