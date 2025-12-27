using CloudStorage.Common.Enums;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace CloudStorage.Common.DTOs
{
    /// <summary>
    /// Represents the data transfer object (DTO) for a file upload operation.
    /// </summary>
    /// <remarks>This class is typically used to encapsulate the data required for uploading a file, such as
    /// file metadata, content, or other related properties.</remarks>
    public class FileUploadDto
    {
        [Required]
        public string? FileName { get; set; } =string.Empty;

        [Required]
        public StorageProviderType ProviderType { get; set; }

        [Required]
        public string CredentialName { get; set; } =string.Empty;
        public string? FolderPath { get; set; }

        [Required]
        public IFormFile? File { get; set; } = null!;

        public string? ContentType { get; set; }

        public Dictionary<string, string>? Metadata { get; set; }



    }
}
