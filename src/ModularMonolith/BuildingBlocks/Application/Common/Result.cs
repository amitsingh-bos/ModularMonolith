namespace ModularMonolith.BuildingBlocks.Application.Common;

public enum ErrorType
{
    None,
    Validation,
    NotFound,
    Unauthorized,
    Forbidden,
    Conflict,
    General
}

public sealed class Result<T>
{
    private Result(bool isSuccess, T? value, string error, ErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T? Value { get; }
    public string Error { get; }
    public ErrorType ErrorType { get; }

    public static Result<T> Success(T value) =>
        new(true, value, string.Empty, ErrorType.None);

    public static Result<T> Failure(string error, ErrorType errorType = ErrorType.General) =>
        new(false, default, error, errorType);
}

public static class Result
{
    public static Result<T> Success<T>(T value) => Result<T>.Success(value);
    public static Result<T> Failure<T>(string error, ErrorType errorType = ErrorType.General) =>
        Result<T>.Failure(error, errorType);
}
