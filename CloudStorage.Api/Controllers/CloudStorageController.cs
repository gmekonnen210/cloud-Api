using CloudStorage.Common.DTOs;
using CloudStorage.Common.Enums;
using CloudStorage.DomainService.Implementations;
using CloudStorage.DomainService.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CloudStorage.Api.Controllers
{
   
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class CloudStorageController : ControllerBase
    {
        private readonly IStorageProvideFactory _providerFactory;
        private readonly ILogger<CloudStorageController> _logger;

        public CloudStorageController(IStorageProvideFactory providerFactory , ILogger<CloudStorageController> logger)
        {
            _logger = logger;
            _providerFactory = providerFactory;

        }

        /// <summary>
        /// Downloads a file from storage
        /// </summary>
        /// <param name="providerType">Storage provider type (AzureBlob = 1, AwsS3 = 2)</param>
        /// <param name="credentialName">Name of the credential configuration</param>
        /// <param name="fileName">Name of the file to download</param>
        /// <param name="folderPath">Optional folder path</param>
        /// <returns>File content</returns>
        [HttpGet("download")]
        [ProducesResponseType(typeof(FileContentResult), 200)]
        [ProducesResponseType(typeof(StorageResponse<object>), 400)]
        [ProducesResponseType(typeof(StorageResponse<object>), 404)]
        [ProducesResponseType(typeof(StorageResponse<object>), 500)]
        public async Task<IActionResult> DownloadFile(
            [FromQuery] StorageProviderType providerType,
            [FromQuery] string credentialName,
            [FromQuery] string fileName,
            [FromQuery] string? folderPath = null)
        {
            try
            {
                _logger.LogInformation($"Download request: Provider={providerType}, File={fileName}");

                var request = new StorageRequest
                {
                    ProviderType = providerType,
                    CredentialName = credentialName,
                    FileName = fileName,
                    FolderPath = folderPath
                };

                var storageService = _providerFactory.CreateStorageService(providerType, credentialName);
                var fileContent = await storageService.DownloadFileAsync(request);

                return File(fileContent, "application/octet-stream", fileName);
            }
            catch (FileNotFoundException ex)
            {
                _logger.LogWarning(ex, $"File not found: {fileName}");
                return NotFound(StorageResponse<object>.CreateError($"File not found: {ex.Message}", "FILE_NOT_FOUND"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid arguments: {ex.Message}");
                return BadRequest(StorageResponse<object>.CreateError($"Invalid request: {ex.Message}", "INVALID_ARGUMENT"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {fileName}");
                return StatusCode(500, StorageResponse<object>.CreateError($"Error downloading file: {ex.Message}", "DOWNLOAD_ERROR"));
            }
        }

        /// <summary>
        /// Uploads a file to storage
        /// </summary>
        /// <param name="uploadDto">File upload details</param>
        /// <returns>Upload result with file location</returns>
        [HttpPost("upload")]
        [ProducesResponseType(typeof(StorageResponse<string>), 200)]
        [ProducesResponseType(typeof(StorageResponse<object>), 400)]
        [ProducesResponseType(typeof(StorageResponse<object>), 500)]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadDto uploadDto)
        {
            try
            {
                _logger.LogInformation($"Upload request: Provider={uploadDto.ProviderType}, File={uploadDto.FileName}");

                var storageService = _providerFactory.CreateStorageService(uploadDto.ProviderType, uploadDto.CredentialName);
                var fileLocation = await storageService.UploadFileAsync(uploadDto);

                return Ok(StorageResponse<string>.CreateSuccess(
                    fileLocation,
                    $"File '{uploadDto.FileName}' uploaded successfully"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid arguments: {ex.Message}");
                return BadRequest(StorageResponse<object>.CreateError($"Invalid request: {ex.Message}", "INVALID_ARGUMENT"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {uploadDto.FileName}");
                return StatusCode(500, StorageResponse<object>.CreateError($"Error uploading file: {ex.Message}", "UPLOAD_ERROR"));
            }
        }

        /// <summary>
        /// Deletes a file from storage
        /// </summary>
        /// <param name="providerType">Storage provider type</param>
        /// <param name="credentialName">Name of the credential configuration</param>
        /// <param name="fileName">Name of the file to delete</param>
        /// <param name="folderPath">Optional folder path</param>
        /// <returns>Deletion result</returns>
        [HttpDelete("delete")]
        [ProducesResponseType(typeof(StorageResponse<bool>), 200)]
        [ProducesResponseType(typeof(StorageResponse<object>), 400)]
        [ProducesResponseType(typeof(StorageResponse<object>), 500)]
        public async Task<IActionResult> DeleteFile(
            [FromQuery] StorageProviderType providerType,
            [FromQuery] string credentialName,
            [FromQuery] string fileName,
            [FromQuery] string? folderPath = null)
        {
            try
            {
                _logger.LogInformation($"Delete request: Provider={providerType}, File={fileName}");

                var request = new StorageRequest
                {
                    ProviderType = providerType,
                    CredentialName = credentialName,
                    FileName = fileName,
                    FolderPath = folderPath
                };

                var storageService = _providerFactory.CreateStorageService(providerType, credentialName);
                var result = await storageService.DeleteFileAsync(request);

                return Ok(StorageResponse<bool>.CreateSuccess(
                    result,
                    result ? $"File '{fileName}' deleted successfully" : $"File '{fileName}' not found"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid arguments: {ex.Message}");
                return BadRequest(StorageResponse<object>.CreateError($"Invalid request: {ex.Message}", "INVALID_ARGUMENT"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {fileName}");
                return StatusCode(500, StorageResponse<object>.CreateError($"Error deleting file: {ex.Message}", "DELETE_ERROR"));
            }
        }

        /// <summary>
        /// Checks if a file exists in storage
        /// </summary>
        /// <param name="providerType">Storage provider type</param>
        /// <param name="credentialName">Name of the credential configuration</param>
        /// <param name="fileName">Name of the file to check</param>
        /// <param name="folderPath">Optional folder path</param>
        /// <returns>Existence check result</returns>
        [HttpGet("exists")]
        [ProducesResponseType(typeof(StorageResponse<bool>), 200)]
        [ProducesResponseType(typeof(StorageResponse<object>), 400)]
        [ProducesResponseType(typeof(StorageResponse<object>), 500)]
        public async Task<IActionResult> FileExists(
            [FromQuery] StorageProviderType providerType,
            [FromQuery] string credentialName,
            [FromQuery] string fileName,
            [FromQuery] string? folderPath = null)
        {
            try
            {
                _logger.LogInformation($"Exists check: Provider={providerType}, File={fileName}");

                var request = new StorageRequest
                {
                    ProviderType = providerType,
                    CredentialName = credentialName,
                    FileName = fileName,
                    FolderPath = folderPath
                };

                var storageService = _providerFactory.CreateStorageService(providerType, credentialName);
                var exists = await storageService.FileExistsAsync(request);

                return Ok(StorageResponse<bool>.CreateSuccess(
                    exists,
                    exists ? $"File '{fileName}' exists" : $"File '{fileName}' does not exist"));
            }
            catch (ArgumentException ex)
            {
                _logger.LogWarning(ex, $"Invalid arguments: {ex.Message}");
                return BadRequest(StorageResponse<object>.CreateError($"Invalid request: {ex.Message}", "INVALID_ARGUMENT"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking file existence {fileName}");
                return StatusCode(500, StorageResponse<object>.CreateError($"Error checking file: {ex.Message}", "CHECK_ERROR"));
            }
        }

        /// <summary>
        /// Gets available credential names for a provider type
        /// </summary>
        /// <param name="providerType">Storage provider type</param>
        [HttpGet("credentials")]
        [ProducesResponseType(typeof(StorageResponse<List<string>>), 200)]
        public IActionResult GetCredentials([FromQuery] StorageProviderType providerType)
        {
            try
            {
                                                  
                var factory = _providerFactory as StorageProviderFactory;
                if (factory == null)
                    return Ok(StorageResponse<List<string>>.CreateSuccess(new List<string>()));

                var credentials = factory.GetCredentialNames(providerType);
                return Ok(StorageResponse<List<string>>.CreateSuccess(credentials));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting credentials");
                return StatusCode(500, StorageResponse<object>.CreateError("Error getting credentials", "CREDENTIALS_ERROR"));
            }
        }
    }
}