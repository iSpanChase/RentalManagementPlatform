using Microsoft.Extensions.Options;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Services
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
