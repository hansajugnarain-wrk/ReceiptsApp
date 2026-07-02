namespace ReceiptsApp.Application.Common.Models;

/// <summary>
/// Represents the outcome of a use case without throwing exceptions for
/// expected failures (wrong password, not found, duplicate email).
/// Domain exceptions still throw — they represent invalid state, not
/// ordinary control flow.
/// </summary>
public class Result<T>
{
    public T? Value { get; }
    public string? Error { get; }
    public bool IsSuccess => Error is null;
    public bool IsFailure => !IsSuccess;

    private Result(T value)
    {
        Value = value;
    }

    private Result(string error)
    {
        Error = error;
    }

    public static Result<T> Success(T value) => new(value);

    public static Result<T> Failure(string error) => new(error);
}

public static class Result
{
    public static Result<T> Ok<T>(T value) => Result<T>.Success(value);

    public static Result<T> Fail<T>(string error) => Result<T>.Failure(error);
}
