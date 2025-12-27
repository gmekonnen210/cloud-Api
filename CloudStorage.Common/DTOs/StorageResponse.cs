namespace CloudStorage.Common.DTOs
{
    public class StorageResponse <T>
    {
        public bool IsSuccess { get; set; }

        public string Message { get; set; } = string.Empty;
       
        public T? Data { get; set; }

        public string? ErrorCode { get; set; }
        public static StorageResponse<T> CreateSuccess(T data, string message = "Operation completed successfully")
        {
            return new StorageResponse<T>
            {
                IsSuccess = true,
                Message = message,
                Data = data
            };
        }
        public static StorageResponse<T> CreateError(string message, string? errorCode = null)
        {
            return new StorageResponse<T>
            {
                IsSuccess = false,
                Message = message,
                ErrorCode = errorCode
            };
        }

    }
}
