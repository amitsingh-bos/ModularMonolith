namespace ModularMonolith.BuildingBlocks.Application.Common;

public sealed class ApiResponse<T> : ApiResponse
{
    public T? Data { get; init; }

    public static ApiResponse<T> Ok(T data, string message = "Success.") => new()
    {
        StatusCode = 200,
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> Created(T data, string message = "Created successfully.") => new()
    {
        StatusCode = 201,
        Success = true,
        Message = message,
        Data = data
    };

    public static ApiResponse<T> OkPaged(T data, PaginationMeta pagination, string message = "Success.") => new()
    {
        StatusCode = 200,
        Success = true,
        Message = message,
        Data = data,
        Pagination = pagination
    };

    public static ApiResponse<T> FromResult(Result<T> result) => result.IsSuccess
        ? Ok(result.Value!)
        : new ApiResponse<T>
        {
            StatusCode = MapErrorType(result.ErrorType),
            Success = false,
            Message = result.Error,
            Errors = [result.Error]
        };

    private static int MapErrorType(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation  => 400,
        ErrorType.NotFound    => 404,
        ErrorType.Unauthorized => 401,
        ErrorType.Forbidden   => 403,
        ErrorType.Conflict    => 409,
        _                     => 500
    };
}
