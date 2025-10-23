using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using RentalManagementPlatformWebAPI.DTOs; // Changed to API's DTOs namespace for MinioSettings
using Microsoft.AspNetCore.StaticFiles;
using RentalManagementPlatformWebAPI.Services.Interfaces; // Added for IMinioService
using System.IO; // Added for Stream
using System.Threading.Tasks; // Added for Task
using System; // Added for Exception

namespace RentalManagementPlatformWebAPI.Services
{
    public class MinioService : IMinioService
    {
        private readonly IMinioClient _minioClient;
        private readonly MinioSettings _minioSettings;

        public MinioService(IOptions<MinioSettings> minioOptions)
        {
            _minioSettings = minioOptions.Value;
            _minioClient = new MinioClient()
                .WithEndpoint(_minioSettings.Endpoint)
                .WithCredentials(_minioSettings.AccessKey, _minioSettings.SecretKey)
                .Build();
        }

        public async Task<string> UploadFileAsync(Stream stream, string fileName)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;

                var beArgs = new BucketExistsArgs().WithBucket(bucketName);
                if (!await _minioClient.BucketExistsAsync(beArgs))
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }

                var provider = new FileExtensionContentTypeProvider();
                if (!provider.TryGetContentType(fileName, out var contentType))
                {
                    contentType = "application/octet-stream";
                }
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                return fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error uploading to MinIO: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetFileUrlAsync(string objectName)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;
                var expiry = 60 * 60;

                var args = new PresignedGetObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(objectName)
                    .WithExpiry(expiry);
                return await _minioClient.PresignedGetObjectAsync(args);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting file URL from MinIO: {ex.Message}");
                throw;
            }
        }
    }
}