
using CloudStorage.Common.Enums;
using System.ComponentModel.DataAnnotations;
namespace CloudStorage.Common.DTOs
{
    /// <summary>
    /// Represents a request to store a file, including metadata and content.
    /// </summary>
    /// <remarks>This class encapsulates the details required for a file storage operation, such as the file's
    /// name,  content type, and the input stream containing the file data. The <see cref="FileName"/> property is 
    /// required for the request to be valid.</remarks>
    public class StorageRequest
    {
        [Required]
        public StorageProviderType ProviderType { get; set; }

        [Required]
        public string CredentialName { get; set; } =string.Empty;

        public string? FolderPath { get; set; }

        [Required]
        public string? FileName { get; set; }
        
    }
}
