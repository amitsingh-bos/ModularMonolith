namespace ModularMonolith.BuildingBlocks.Application.Common;

public class ApiResponse
{
    public int StatusCode { get; init; }
    public bool Success { get; init; }
    public string Message { get; init; } = string.Empty;
    public IReadOnlyList<string> Errors { get; init; } = [];
    public PaginationMeta? Pagination { get; init; }
    public string? CorrelationId { get; init; }

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
