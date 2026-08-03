using Bookings.Common.Exceptions;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using System;
using System.Threading.Tasks;

namespace BookingService.Api.Middleware;

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
        catch (DbUpdateConcurrencyException ex)
        {
            _logger.LogWarning(ex, "Booking update conflict for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                "Booking update conflict",
                "The booking was changed by another request. Reload and try again.");
        }
        catch (BusinessRuleException ex)
        {
            _logger.LogWarning(ex, "Business rule violation for {Method} {Path}",
                context.Request.Method, context.Request.Path);

            await WriteProblemDetailsAsync(
                context,
                StatusCodes.Status409Conflict,
                "Business rule violation",
                ex.Message);
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
            _logger.LogError(ex, "Unhandled exception for {Method} {Path}",
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
