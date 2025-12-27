using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using CloudStorage.Common.DTOs;
using CloudStorage.DomainService.Interfaces;
using Microsoft.Extensions.Logging;

namespace CloudStorage.DomainService.Implementations
{
    public class AzureStorageProvider : BaseStorageProvider ,IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        private readonly ILogger<AzureStorageProvider> _logger;
        public AzureStorageProvider(string connectionString, string containerName, ILogger<AzureStorageProvider> logger)
            : base(connectionString, containerName)
        {
            _blobServiceClient = new BlobServiceClient(connectionString);
            _logger = logger;

            //Ensure container existsInitializeContainerAsync().Wait();

            InitializeContainerAsync().Wait();
        }
        

        private async Task InitializeContainerAsync()
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                await containerClient.CreateIfNotExistsAsync();
                _logger.LogInformation($"Container '{_containerName}' is ready for use.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error initializing container '{_containerName}': {ex.Message}");
                throw;
            }
        }
        public override async Task<byte[]> DownloadFileAsync(StorageRequest request)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(GetFullPath(request.FileName, request.FolderPath));

                if (!await blobClient.ExistsAsync())
                    throw new FileNotFoundException($"File {request.FileName} not found in Azure container {_containerName}");

                using var memoryStream = new MemoryStream();
                await blobClient.DownloadToAsync(memoryStream);
                _logger.LogInformation($"Downloaded file {request.FileName} from Azure");

                return memoryStream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {request.FileName} from Azure");
                throw;
            }
        }

        public override async Task<string> UploadFileAsync(FileUploadDto uploadDto)
        {
            ValidateUploadDto(uploadDto);

            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(GetFullPath(uploadDto.FileName, uploadDto.FolderPath));

                var blobHttpHeaders = new BlobHttpHeaders
                {
                    ContentType = uploadDto.ContentType ?? "application/octet-stream"
                };

                var metadata = uploadDto.Metadata ?? new Dictionary<string, string>();
                metadata["UploadTimestamp"] = DateTime.UtcNow.ToString("o");
                metadata["OriginalFileName"] = uploadDto.File.FileName;

                await using var stream = uploadDto.File.OpenReadStream();
                await blobClient.UploadAsync(stream, new BlobUploadOptions
                {
                    HttpHeaders = blobHttpHeaders,
                    Metadata = metadata
                });

                _logger.LogInformation($"Uploaded file {uploadDto.FileName} to Azure");
                return blobClient.Uri.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {uploadDto.FileName} to Azure");
                throw;
            }
        }

        public override async Task<bool> DeleteFileAsync(StorageRequest request)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(GetFullPath(request.FileName, request.FolderPath));

                var response = await blobClient.DeleteIfExistsAsync();
                _logger.LogInformation($"Deleted file {request.FileName} from Azure: {response.Value}");

                return response.Value;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {request.FileName} from Azure");
                throw;
            }
        }

        public override async Task<bool> FileExistsAsync(StorageRequest request)
        {
            try
            {
                var containerClient = _blobServiceClient.GetBlobContainerClient(_containerName);
                var blobClient = containerClient.GetBlobClient(GetFullPath(request.FileName, request.FolderPath));

                return await blobClient.ExistsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking file existence {request.FileName} in Azure");
                throw;
            }
        }
    }
}

