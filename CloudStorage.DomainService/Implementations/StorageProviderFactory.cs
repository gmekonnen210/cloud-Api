using CloudStorage.Common.Config;
using CloudStorage.Common.Enums;
using CloudStorage.DomainService.Interfaces;
using Microsoft.Extensions.Logging;

namespace CloudStorage.DomainService.Implementations
{
    /// <summary>
    /// Provides a factory for creating storage service instances based on the specified provider type and credentials.
    /// </summary>
    /// <remarks>This factory supports creating storage services for multiple provider types, such as Azure
    /// Blob Storage and AWS S3. The credentials for each provider are pre-configured and identified by a unique
    /// credential name.</remarks>
    public class StorageProviderFactory : IStorageProvideFactory
    {
        private readonly ILogger<StorageProviderFactory> _logger;
        private readonly ILogger<AwsS3StorageProvider> _awsLogger;
        private readonly ILogger<AzureStorageProvider> _azureLogger;

        // Hard-coded credentials dictionary as requested
        // In production, these should be stored in secure configuration (Azure Key Vault, AWS Secrets Manager, etc.)
        private readonly Dictionary<string, StorageCredential> _credentials = new()
        {
            {
                "azure-dev",
                new StorageCredential
                {
                    CredentialName = "azure-dev",
                    ProviderType = StorageProviderType.AzureBlob,
                    ConnectionString = "DefaultEndpointsProtocol=https;AccountName=devstorageaccount;AccountKey=your-account-key;EndpointSuffix=core.windows.net",
                    ContainerName = "dev-container"
                }
            },
            {
                "aws-dev",
                new StorageCredential
                {
                    CredentialName = "aws-dev",
                    ProviderType = StorageProviderType.AwsS3,
                    AccessKey = "your-access-key",
                    SecretKey = "your-secret-key",
                    Region = "us-east-1",
                    BucketName = "dev-bucket"
                }
            },
            // Future: Add Google Cloud credentials here
        };
        public IStorageService CreateStorageService(StorageProviderType providerType, string credentialName)
        {
            if (!_credentials.TryGetValue(credentialName, out var credential))
            {
                throw new ArgumentException($"Credential '{credentialName}' not found");
            }

            if (credential.ProviderType != providerType)
            {
                throw new ArgumentException($"Credential '{credentialName}' is not configured for provider {providerType}");
            }

            return providerType switch
            {
                StorageProviderType.AzureBlob => CreateAzureProvider(credential),
                StorageProviderType.AwsS3 => CreateAwsProvider(credential),
                _ => throw new NotSupportedException($"Provider type {providerType} is not supported")
            };
        }

        private AzureStorageProvider CreateAzureProvider(StorageCredential credential)
        {
            if (string.IsNullOrEmpty(credential.ConnectionString))
                throw new ArgumentException("Azure connection string is required");

            if (string.IsNullOrEmpty(credential.ContainerName))
                throw new ArgumentException("Azure container name is required");

            _logger.LogInformation($"Creating Azure Storage Provider for container '{credential.ContainerName}'");

            return new AzureStorageProvider(
                credential.ConnectionString,
                credential.ContainerName,
                _azureLogger);
        }

        private AwsS3StorageProvider CreateAwsProvider(StorageCredential credential)
        {
            if (string.IsNullOrEmpty(credential.AccessKey))
                throw new ArgumentException("AWS Access Key is required");

            if (string.IsNullOrEmpty(credential.SecretKey))
                throw new ArgumentException("AWS Secret Key is required");

            if (string.IsNullOrEmpty(credential.Region))
                throw new ArgumentException("AWS Region is required");

            if (string.IsNullOrEmpty(credential.BucketName))
                throw new ArgumentException("AWS Bucket Name is required");

            _logger.LogInformation($"Creating AWS S3 Provider for bucket '{credential.BucketName}' in region '{credential.Region}'");

            return new AwsS3StorageProvider(
                credential.AccessKey,
                credential.SecretKey,
                credential.Region,
                credential.BucketName,
                _awsLogger);
        }

        /// <summary>
        /// Gets all available credential names for a provider type
        /// </summary>
        public List<string> GetCredentialNames(StorageProviderType providerType)
        {
            return _credentials
                .Where(kv => kv.Value.ProviderType == providerType)
                .Select(kv => kv.Key)
                .ToList();
        }
    }
}