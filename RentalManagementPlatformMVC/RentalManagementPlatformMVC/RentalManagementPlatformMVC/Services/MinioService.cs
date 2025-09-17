using Microsoft.Extensions.Options;
using Minio;
using Minio.DataModel.Args;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
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

        public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;

                // Check if the bucket exists, create it if not.
                var beArgs = new BucketExistsArgs().WithBucket(bucketName);
                bool found = await _minioClient.BucketExistsAsync(beArgs);
                if (!found)
                {
                    var mbArgs = new MakeBucketArgs().WithBucket(bucketName);
                    await _minioClient.MakeBucketAsync(mbArgs);
                }

                // Upload the file
                var putObjectArgs = new PutObjectArgs()
                    .WithBucket(bucketName)
                    .WithObject(fileName)
                    .WithStreamData(stream)
                    .WithObjectSize(stream.Length)
                    .WithContentType(contentType);

                await _minioClient.PutObjectAsync(putObjectArgs);

                // For simplicity, we'll just return the object name. 
                // In a real app, you might return the full URL.
                return fileName;
            }
            catch (Exception ex)
            {
                // In a real app, you'd want to log this exception.
                Console.WriteLine($"Error uploading to MinIO: {ex.Message}");
                throw;
            }
        }

        public async Task<string> GetFileUrlAsync(string objectName)
        {
            try
            {
                var bucketName = _minioSettings.BucketName;
                var expiry = 60 * 60; // URL aקס expir in 1 hour

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
