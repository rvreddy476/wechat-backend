namespace Shared.Contracts.Common;

public class ApiResponse<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public List<string>? Errors { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public static ApiResponse<T> SuccessResponse(T data) => new() { Success = true, Data = data };
    public static ApiResponse<T> ErrorResponse(string error) => new() { Success = false, Error = error };
    public static ApiResponse<T> ErrorResponse(List<string> errors) => new() { Success = false, Errors = errors };
}
