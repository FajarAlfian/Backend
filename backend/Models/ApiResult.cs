using System;

namespace DlanguageApi.Models
{
    public class ApiResult<T>
    {
        public bool Success { get; set; }
        public T? Data { get; set; }
        public string? Message { get; set; }
        public List<string>? Errors { get; set; }
        public int StatusCode { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public static ApiResult<T> SuccessResult(T data, string? message = null, int statusCode = 200)
        {
            return new ApiResult<T>
            {
                Success = true,
                Data = data,
                Message = message ?? "Operation completed successfully",
                StatusCode = statusCode
            };
        }

        public static ApiResult<T> SuccessResult(string message = "Operation completed successfully", int statusCode = 200)
        {
            return new ApiResult<T>
            {
                Success = true,
                Message = message,
                StatusCode = statusCode
            };
        }

        public static ApiResult<T> Error(string error, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Errors = new List<string> { error },
                Message = "Operation failed",
                StatusCode = statusCode
            };
        }

        public static ApiResult<T> Error(List<string> errors, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Errors = errors,
                Message = "Operation failed",
                StatusCode = statusCode
            };
        }

        public static ApiResult<T> Error(string message, List<string> errors, int statusCode = 400)
        {
            return new ApiResult<T>
            {
                Success = false,
                Message = message,
                Errors = errors,
                StatusCode = statusCode
            };
        }
    }
}



