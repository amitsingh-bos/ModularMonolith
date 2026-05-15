using FluentValidation;
using Microsoft.EntityFrameworkCore;
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

        // DbUpdateConcurrencyException is handled first: load current DB values for the 409 body.
        if (ex is DbUpdateConcurrencyException concurrencyEx)
        {
            var currentValues = await LoadCurrentDbValuesAsync(concurrencyEx);
            context.Response.StatusCode = 409;
            context.Response.ContentType = "application/json";
            await context.Response.WriteAsJsonAsync(
                ApiResponse.ConcurrencyConflict(currentValues, correlationId));
            return;
        }

        // Specific domain exceptions must come before the base DomainException catch-all.
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

    private static async Task<IReadOnlyDictionary<string, object?>?> LoadCurrentDbValuesAsync(
        DbUpdateConcurrencyException ex)
    {
        var entry = ex.Entries.FirstOrDefault();
        if (entry is null) return null;

        var dbValues = await entry.GetDatabaseValuesAsync();
        if (dbValues is null) return null;

        // Return only mapped (non-shadow) scalar properties in camelCase so the
        // JSON matches the API's standard property naming convention.
        return dbValues.Properties
            .Where(p => !p.IsShadowProperty())
            .ToDictionary(
                p => char.ToLowerInvariant(p.Name[0]) + p.Name[1..],
                p => dbValues[p],
                StringComparer.Ordinal);
    }
}
