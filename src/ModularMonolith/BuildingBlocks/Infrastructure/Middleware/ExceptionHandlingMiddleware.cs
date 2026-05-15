using FluentValidation;
using ModularMonolith.BuildingBlocks.Application.Common;
using ModularMonolith.BuildingBlocks.Domain.Exceptions;
using ModularMonolith.Modules.Auth.Domain.Exceptions;

namespace ModularMonolith.BuildingBlocks.Infrastructure.Middleware;

public sealed class ExceptionHandlingMiddleware
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

    private static async Task HandleExceptionAsync(HttpContext context, Exception ex)
    {
        var correlationId = context.Items[CorrelationIdMiddleware.ItemKey] as string;

        // specific domain exceptions must come before the base DomainException catch-all
        var (statusCode, message) = ex switch
        {
            ValidationException ve => (400, string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))),
            InvalidCredentialsException ice => (401, ice.Message),
            InvalidTokenException ite => (401, ite.Message),
            AccountLockedException ale => (423, ale.Message),
            UserAlreadyExistsException uae => (409, uae.Message),
            TenantInactiveException tie => (403, tie.Message),
            DomainException de => (400, de.Message),
            NotFoundException nfe => (404, nfe.Message),
            UnauthorizedAccessException => (401, "Unauthorized."),
            _ => (500, "An unexpected error occurred.")
        };

        context.Response.StatusCode = statusCode;
        context.Response.ContentType = "application/json";
        await context.Response.WriteAsJsonAsync(ApiResponse.Fail(message, statusCode, correlationId));
    }
}
