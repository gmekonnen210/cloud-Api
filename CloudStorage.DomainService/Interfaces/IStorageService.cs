using CloudStorage.Common.DTO;
using CloudStorage.Common.DTOs;

namespace CloudStorage.DomainService.Interfaces
{
    /// <summary>
    /// Defines methods for uploading, downloading, and deleting files in a storage system.
    /// </summary>
    /// <remarks>This interface provides an abstraction for interacting with a storage system, allowing files
    /// to be uploaded, downloaded, and deleted. Implementations of this interface may target different storage
    /// providers, such as cloud-based or local storage solutions.</remarks>
    public interface IStorageService
    {

        /// <summary>
        /// Downloads a file from storage
        /// </summary>
        /// <param name="request">Storage request containing provider and file details</param>
        /// <returns>File content as byte array</returns>
        Task<byte[]> DownloadFileAsync(StorageRequest request);

        /// <summary>
        /// Uploads a file to storage
        /// </summary>
        /// <param name="uploadDto">File upload details</param>
        /// <returns>File path/identifier</returns>
        Task<string> UploadFileAsync(FileUploadDto uploadDto);

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="request">Storage request containing provider and file details</param>
        /// <returns>True if deletion was successful</returns>
        Task<bool> DeleteFileAsync(StorageRequest request);


        /// <summary>
        /// Checks if a file exists in storage
        /// </summary>
        /// <param name="request">Storage request containing provider and file details</param>
        /// <returns>True if file exists</returns>
        Task<bool> FileExistsAsync(StorageRequest request);
    }
}
