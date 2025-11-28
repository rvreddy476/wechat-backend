namespace AuthService.Application.Common;

public class Result<T>
{
    public bool IsSuccess { get; private set; }
    public T? Value { get; private set; }
    public string? Error { get; private set; }
    public string? Message { get; private set; }

    private Result(bool isSuccess, T? value, string? error, string? message = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        Message = message;
    }

    public static Result<T> Success(T value, string? message = null)
    {
        return new Result<T>(true, value, null, message);
    }

    public static Result<T> Failure(string error)
    {
        return new Result<T>(false, default, error, null);
    }
}

public class Result
{
    public bool IsSuccess { get; private set; }
    public string? Error { get; private set; }
    public string? Message { get; private set; }

    private Result(bool isSuccess, string? error, string? message = null)
    {
        IsSuccess = isSuccess;
        Error = error;
        Message = message;
    }

    public static Result Success(string? message = null)
    {
        return new Result(true, null, message);
    }

    public static Result Failure(string error)
    {
        return new Result(false, error, null);
    }
}
