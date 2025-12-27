
using CloudStorage.Common.DTOs;
using CloudStorage.DomainService.Interfaces;

namespace CloudStorage.DomainService.Implementations
{
    /// <summary>
    /// provinding base common functionality for all storage providers .
    /// a common base implementation for storage services, enabling file upload, download, and deletion operations
    /// across different storage providers.
    /// </summary>
    /// <remarks>This class serves as a foundational service for interacting with storage systems. It supports
    /// operations such as uploading, downloading, and deleting files, and dynamically resolves the appropriate storage
    /// repository service based on the provided connection name.</remarks>
    public abstract class BaseStorageProvider : IStorageService
    {
        
        protected readonly string _containerName;
        protected readonly string _bucketName;

        protected BaseStorageProvider(string containerName, string bucketName)
        {
            _containerName = containerName;
            _bucketName = bucketName;
        }

        /// <summary>
        /// Constructs the full path for a file
        /// </summary>
        protected string GetFullPath(string fileName, string? folderPath)
        {
            if (string.IsNullOrEmpty(folderPath))
                return fileName;

            return folderPath.EndsWith('/')
                ? $"{folderPath}{fileName}"
                : $"{folderPath}/{fileName}";
        }

        // Abstract methods that derived classes must implement
        public abstract Task<byte[]> DownloadFileAsync(StorageRequest request);
        public abstract Task<string> UploadFileAsync(FileUploadDto uploadDto);
        public abstract Task<bool> DeleteFileAsync(StorageRequest request);
        public abstract Task<bool> FileExistsAsync(StorageRequest request);
        /// <summary>
        /// Validates upload DTO
        /// </summary>
        protected void ValidateUploadDto(FileUploadDto uploadDto)
        {
            if (uploadDto.File == null || uploadDto.File.Length == 0)
                throw new ArgumentException("File cannot be empty");

            if (string.IsNullOrEmpty(uploadDto.FileName))
                throw new ArgumentException("File name is required");
        }
    }
}
