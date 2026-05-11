namespace Duotify.Membership.Web.Application.Services;

public class ServiceResult
{
    public bool Succeeded { get; init; }
    public string? ErrorCode { get; init; }
    public string? Message { get; init; }

    public static ServiceResult Success() => new() { Succeeded = true };
    public static ServiceResult Failure(string errorCode, string message) => new() { Succeeded = false, ErrorCode = errorCode, Message = message };
}

public sealed class ServiceResult<T> : ServiceResult
{
    public T? Value { get; init; }

    public static ServiceResult<T> Success(T value) => new() { Succeeded = true, Value = value };
    public new static ServiceResult<T> Failure(string errorCode, string message) => new() { Succeeded = false, ErrorCode = errorCode, Message = message };
}