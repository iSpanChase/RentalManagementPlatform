using Microsoft.Extensions.Options;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Services
{
    public class ImageUrlResolver : IImageUrlResolver
    {
        private readonly MinioSettings _minioSettings;

        public ImageUrlResolver(IOptions<MinioSettings> minioSettings)
        {
            _minioSettings = minioSettings.Value;
        }

        public string ResolveImageUrl(RoomPhoto photo)
        {
            // Assuming Minio endpoint is publicly accessible and directly serves content
            // The URL format is typically http://<endpoint>/<bucketName>/<objectKey>
            // Ensure your Minio setup allows public access or use presigned URLs for private buckets
            return $"{_minioSettings.Endpoint}/{photo.Bucket}/{photo.ObjectKey}";
        }
    }
}
