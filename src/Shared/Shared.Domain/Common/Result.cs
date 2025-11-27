namespace Shared.Domain.Common;

/// <summary>
/// Result pattern for better error handling without exceptions
/// </summary>
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        if (isSuccess && !string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Success result cannot have an error");
        if (!isSuccess && string.IsNullOrEmpty(error))
            throw new InvalidOperationException("Failure result must have an error");

        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
    public static Result<T> Success<T>(T value) => new(value, true, string.Empty);
    public static Result<T> Failure<T>(string error) => new(default!, false, error);
}

/// <summary>
/// Result pattern with return value
/// </summary>
public class Result<T> : Result
{
    public T Value { get; }

    protected internal Result(T value, bool isSuccess, string error)
        : base(isSuccess, error)
    {
        Value = value;
    }

    public static implicit operator Result<T>(T value) => Success(value);
}

/// <summary>
/// Error types for standardized error handling
/// </summary>
public static class Errors
{
    public static class Authentication
    {
        public static string InvalidCredentials => "Invalid email or password";
        public static string UserNotFound => "User not found";
        public static string AccountLocked => "Account is locked due to multiple failed login attempts";
        public static string EmailNotVerified => "Email address is not verified";
        public static string InvalidToken => "Invalid or expired token";
        public static string DuplicateEmail => "Email already registered";
        public static string DuplicateUsername => "Username already taken";
        public static string InvalidPassword => "Invalid current password";
    }

    public static class Validation
    {
        public static string Required(string field) => $"{field} is required";
        public static string InvalidFormat(string field) => $"{field} has invalid format";
        public static string TooLong(string field, int maxLength) => $"{field} cannot exceed {maxLength} characters";
        public static string TooShort(string field, int minLength) => $"{field} must be at least {minLength} characters";
    }

    public static class NotFound
    {
        public static string Entity(string entityName, string id) => $"{entityName} with ID '{id}' not found";
        public static string Entity(string entityName) => $"{entityName} not found";
        public static string User => "User not found";
    }

    public static class Conflict
    {
        public static string AlreadyExists(string entityName, string field) => $"{entityName} with this {field} already exists";
        public static string DuplicateEntry => "This entry already exists";
    }

    public static class Authorization
    {
        public static string Unauthorized => "You are not authorized to perform this action";
        public static string Forbidden => "Access to this resource is forbidden";
    }
}
