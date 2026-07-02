using System.Net;
using System.Text.Json;
using FluentValidation;
using Microsoft.Extensions.Logging;
using ReceiptsApp.Domain.Exceptions;

namespace ReceiptsApp.Api.Middleware;

/// <summary>
/// Single place that converts exceptions into JSON HTTP responses, so
/// controllers and handlers never need try/catch for HTTP status mapping.
/// </summary>
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
        catch (ValidationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, new
            {
                error = "Validation failed.",
                details = ex.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
            });
        }
        catch (DomainValidationException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.BadRequest, new { error = ex.Message });
        }
        catch (EntityNotFoundException ex)
        {
            await WriteResponseAsync(context, HttpStatusCode.NotFound, new { error = ex.Message });
        }
        catch (UnauthorizedAccessException)
        {
            await WriteResponseAsync(
                context, HttpStatusCode.Unauthorized, new { error = "Authentication required." });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unhandled exception processing {Path}", context.Request.Path);
            await WriteResponseAsync(
                context, HttpStatusCode.InternalServerError, new { error = "An unexpected error occurred." });
        }
    }

    private static Task WriteResponseAsync(HttpContext context, HttpStatusCode statusCode, object payload)
    {
        context.Response.StatusCode = (int)statusCode;
        context.Response.ContentType = "application/json";
        return context.Response.WriteAsync(JsonSerializer.Serialize(payload));
    }
}
