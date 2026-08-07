using System.Text.Json;
using FinanceDAMT.Application.Common.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FinanceDAMT.API.Middleware;

/// <summary>
/// Captures unhandled exceptions and converts them to RFC7807 problem responses.
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly bool _exposeErrors;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlobalExceptionHandlerMiddleware"/> class.
    /// </summary>
    /// <param name="next">Next middleware in pipeline.</param>
    /// <param name="logger">Structured logger instance.</param>
    /// <param name="environment">Host environment (to expose errors in Development).</param>
    /// <param name="configuration">App configuration (reads Debugging:ExposeErrors).</param>
    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment,
        IConfiguration configuration)
    {
        _next = next;
        _logger = logger;
        // Opt-in: surface the real exception message on 500s for diagnostics.
        // Enabled automatically in Development, or in any environment by setting
        // Debugging:ExposeErrors=true (e.g. env var Debugging__ExposeErrors=true).
        _exposeErrors = environment.IsDevelopment()
            || (bool.TryParse(configuration["Debugging:ExposeErrors"], out var expose) && expose);
    }

    /// <summary>
    /// Executes middleware logic for the current HTTP request.
    /// </summary>
    /// <param name="context">Current HTTP context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {

            if (ex is ValidationException or NotFoundException or UnauthorizedException or ConflictException)
            {
                _logger.LogWarning(
                    "{ExceptionType} on {Method} {Path}: {Message}",
                    ex.GetType().Name, context.Request.Method, context.Request.Path, ex.Message);
            }
            else
            {
                _logger.LogError(ex, "Unhandled exception for {Method} {Path}", context.Request.Method, context.Request.Path);
            }

            await HandleExceptionAsync(context, ex, _exposeErrors);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception, bool exposeErrors)
    {
        context.Response.ContentType = "application/problem+json";

        var (statusCode, problem) = exception switch
        {
            ValidationException validationEx => (StatusCodes.Status400BadRequest, new ValidationProblemDetails(validationEx.Errors)
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Failed",
                Detail = validationEx.Message
            }),
            NotFoundException notFoundEx => (StatusCodes.Status404NotFound, new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Not Found",
                Detail = notFoundEx.Message
            }),
            UnauthorizedException unauthorizedEx => (StatusCodes.Status401Unauthorized, new ProblemDetails
            {
                Status = StatusCodes.Status401Unauthorized,
                Title = "Unauthorized",
                Detail = unauthorizedEx.Message
            }),
            ConflictException conflictEx => (StatusCodes.Status409Conflict, new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Conflict",
                Detail = conflictEx.Message
            }),
            _ => (StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Status = StatusCodes.Status500InternalServerError,
                Title = "Internal Server Error",
                Detail = exposeErrors
                    ? $"{exception.GetType().Name}: {exception.Message}"
                    : "An unexpected error occurred."
            })
        };

        context.Response.StatusCode = statusCode;

        var json = JsonSerializer.Serialize(problem, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}
