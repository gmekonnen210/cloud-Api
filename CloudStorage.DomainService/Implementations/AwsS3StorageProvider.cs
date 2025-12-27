using System;
using System.IO;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amazon;
using Amazon.S3;
using Amazon.S3.Model;
using CloudStorage.Common.DTOs;
using CloudStorage.DomainService.Interfaces;
using Microsoft.Extensions.Logging;

namespace CloudStorage.DomainService.Implementations
{
    public class AwsS3StorageProvider : BaseStorageProvider, IStorageService
    {
        private readonly IAmazonS3 _s3Client;
        private readonly ILogger<AwsS3StorageProvider> _logger;
        private readonly string _bucketName;

        public AwsS3StorageProvider(string accessKey,
             string secretKey,
             string region,
             string bucketName, ILogger<AwsS3StorageProvider> logger)
             : base(string.Empty, bucketName)
        {
            var regionEndPoint = RegionEndpoint.GetBySystemName(region);
            _s3Client = new AmazonS3Client(accessKey, secretKey, regionEndPoint);

            //(new BasicAWSCredentials(accessKey, secretKey), regionEndPoint);
            _bucketName = bucketName;
            _logger = logger;

            // Ensure Bucket exists - start initialization without blocking the constructor
            _ = InitializeBucketAsync();
        }

        private async Task InitializeBucketAsync()
        {

            try
            {
                var bucketExists = await Amazon.S3.Util.AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, _bucketName);
                if (!bucketExists)
                {
                    var putBucketRequest = new PutBucketRequest
                    {
                        BucketName = _bucketName,
                        UseClientRegion = true
                    };
                    await _s3Client.PutBucketAsync(putBucketRequest);
                }
                _logger.LogInformation($"AWS S3 bucket '{_bucketName}' initialized");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Failed to initialize AWS S3 bucket '{_bucketName}'");
                throw;
            }
        }
        public override async Task<byte[]> DownloadFileAsync(StorageRequest request)
        {
            try
            {
                var getObjectRequest = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetFullPath(request.FileName, request.FolderPath)
                };

                using var response = await _s3Client.GetObjectAsync(getObjectRequest);
                await using var memoryStream = new MemoryStream();

                await response.ResponseStream.CopyToAsync(memoryStream);
                _logger.LogInformation($"Downloaded file {request.FileName} from AWS S3");

                return memoryStream.ToArray();
            }
            catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                throw new FileNotFoundException($"File {request.FileName} not found in S3 bucket {_bucketName}", ex);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error downloading file {request.FileName} from AWS S3");
                throw;
            }
        }

        public override async Task<string> UploadFileAsync(FileUploadDto uploadDto)
        {
            ValidateUploadDto(uploadDto);

            try
            {
                var putObjectRequest = new PutObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetFullPath(uploadDto.FileName, uploadDto.FolderPath),
                    InputStream = uploadDto.File.OpenReadStream(),
                    ContentType = uploadDto.ContentType ?? "application/octet-stream",
                    AutoCloseStream = true
                };

                // Add metadata
                var metadata = uploadDto.Metadata ?? new Dictionary<string, string>();
                metadata["UploadTimestamp"] = DateTime.UtcNow.ToString("o");
                metadata["OriginalFileName"] = uploadDto.File.FileName;

                foreach (var kvp in metadata)
                {
                    putObjectRequest.Metadata.Add(kvp.Key, kvp.Value);
                }

                var response = await _s3Client.PutObjectAsync(putObjectRequest);
                _logger.LogInformation($"Uploaded file {uploadDto.FileName} to AWS S3. ETag: {response.ETag}");

                return $"s3://{_bucketName}/{GetFullPath(uploadDto.FileName, uploadDto.FolderPath)}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error uploading file {uploadDto.FileName} to AWS S3");
                throw;
            }
        }

        public override async Task<bool> DeleteFileAsync(StorageRequest request)
        {
            try
            {
                var deleteObjectRequest = new DeleteObjectRequest
                {
                    BucketName = _bucketName,
                    Key = GetFullPath(request.FileName, request.FolderPath)
                };

                await _s3Client.DeleteObjectAsync(deleteObjectRequest);
                _logger.LogInformation($"Deleted file {request.FileName} from AWS S3");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting file {request.FileName} from AWS S3");
                throw;
            }
        }

        public override async Task<bool> FileExistsAsync(StorageRequest request)
        {
            try
            {
                var listObjectsRequest = new ListObjectsV2Request
                {
                    BucketName = _bucketName,
                    Prefix = GetFullPath(request.FileName, request.FolderPath),
                    MaxKeys = 1
                };

                var response = await _s3Client.ListObjectsV2Async(listObjectsRequest);
                return response.S3Objects.Count > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error checking file existence {request.FileName} in AWS S3");
                throw;
            }
        }
    }
}