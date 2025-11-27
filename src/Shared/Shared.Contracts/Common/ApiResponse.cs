namespace Shared.Contracts.Common;

/// <summary>
/// Standard API response wrapper
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data)
    {
        return new ApiResponse<T>
        {
            Success = true,
            Data = data
        };
    }

    public static ApiResponse<T> ErrorResponse(string error)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Error = error
        };
    }

    public static ApiResponse<T> ErrorResponse(List<string> errors)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Errors = errors
        };
    }
}

/// <summary>
/// API response without data
/// </summary>
public class ApiResponse : ApiResponse<object>
{
    public static ApiResponse SuccessResponse()
    {
        return new ApiResponse
        {
            Success = true
        };
    }

    public new static ApiResponse ErrorResponse(string error)
    {
        return new ApiResponse
        {
            Success = false,
            Error = error
        };
    }
}
