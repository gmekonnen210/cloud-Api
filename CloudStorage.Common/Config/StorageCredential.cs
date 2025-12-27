using CloudStorage.Common.Enums;
using System.ComponentModel.DataAnnotations;

namespace CloudStorage.Common.Config
{
    /// <summary>
    /// Represents storage credentials for cloud providers
    /// </summary>
    public class StorageCredential
    {
        [Required]
        public string CredentialName { get; set; } = string.Empty;

        [Required]
        public StorageProviderType ProviderType { get; set; }

        // Azure Blob Storage Credentials
        public string? ConnectionString { get; set; }
        public string? AccountName { get; set; }
        public string? AccountKey { get; set; }

        // AWS S3 Credentials
        public string? AccessKey { get; set; }
        public string? SecretKey { get; set; }
        public string? Region { get; set; }

        // Common
        public string? ContainerName { get; set; }
        public string? BucketName { get; set; }
    }
}
