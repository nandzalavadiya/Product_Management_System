namespace CRN_Technical_Assessment.Application.DTOs;

/// <summary>Consistent API response wrapper for single-item results.</summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }
    public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();

    public static ApiResponse<T> Ok(T data, string message = "Success.") =>
        new() { Success = true, Message = message, Data = data };

    public static ApiResponse<T> Fail(string message, IEnumerable<string>? errors = null) =>
        new() { Success = false, Message = message, Errors = errors ?? Enumerable.Empty<string>() };
}
