namespace ModularMonolith.BuildingBlocks.Application.Common;

public class ApiResponse
{
    public int StatusCode { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = [];
    public PaginationMeta? Pagination { get; init; }
    public string? CorrelationId { get; init; }

    // Populated on 409 Conflict (optimistic concurrency) — contains the current DB values
    // so the client can see what changed without making a separate GET request.
    public IReadOnlyDictionary<string, object?>? CurrentValues { get; init; }

    public static ApiResponse NoContent(string message = "No content.") => new()
    {
        StatusCode = 204,
        Success = true,
        Message = message
    };

    public static ApiResponse Fail(string error, int statusCode = 400, string? correlationId = null) => new()
    {
        StatusCode = statusCode,
        Success = false,
        Message = error,
        Errors = [error],
        CorrelationId = correlationId
    };

    public static ApiResponse ConcurrencyConflict(
        IReadOnlyDictionary<string, object?>? currentValues = null,
        string? correlationId = null) => new()
    {
        StatusCode = 409,
        Success = false,
        Message = "The record has been modified by another user. Please refresh and try again.",
        Errors = ["The record has been modified by another user. Please refresh and try again."],
        CurrentValues = currentValues,
        CorrelationId = correlationId
    };

    public static ApiResponse ValidationError(IEnumerable<string> errors, string? correlationId = null)
    {
        var list = errors.ToList();
        return new ApiResponse
        {
            StatusCode = 400,
            Success = false,
            Message = "Validation failed.",
            Errors = list,
            CorrelationId = correlationId
        };
    }
}
