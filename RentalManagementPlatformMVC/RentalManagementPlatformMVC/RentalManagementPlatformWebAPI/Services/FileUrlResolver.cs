using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services
{
    public class FileUrlResolver : IFileUrlResolver
    {
        private readonly RentalManagementPlatformSqlContext _context; // Changed to API's DbContext
        private readonly IMinioService _minioService; // Assuming IMinioService will be in API's Services.Interfaces

        public FileUrlResolver(RentalManagementPlatformSqlContext context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        public async Task<string?> GetUrlAsync(string entityType, int entityId, string photoType)
        {
            var photo = await ResolvePhotoAsync(entityType, entityId, photoType);
            return await GetPhotoUrlAsync(photo);
        }

        public async Task<string?> GetPhotoUrlAsync(RoomPhoto? photo)
        {
            if (photo == null)
            {
                return null;
            }

            if (!string.IsNullOrEmpty(photo.ObjectKey))
            {
                return await _minioService.GetFileUrlAsync(photo.ObjectKey);
            }

            if (!photo.RoomId.HasValue || string.IsNullOrWhiteSpace(photo.PhotoType))
            {
                return null;
            }

            var fallback = await ResolvePhotoAsync("Room", photo.RoomId.Value, photo.PhotoType);
            if (fallback != null && !string.IsNullOrEmpty(fallback.ObjectKey))
            {
                return await _minioService.GetFileUrlAsync(fallback.ObjectKey);
            }

            return null;
        }

        public async Task<IReadOnlyList<string>> GetRoomPhotoUrlsAsync(int roomId)
        {
            var photos = await _context.RoomPhotos
                .Where(p => p.RoomId == roomId)
                .OrderBy(p => p.SortOrder)
                .ThenBy(p => p.PhotoId)
                .ToListAsync();

            var results = new List<string>();
            foreach (var photo in photos)
            {
                var url = await GetPhotoUrlAsync(photo);
                if (!string.IsNullOrEmpty(url))
                {
                    results.Add(url);
                }
            }

            return results;
        }

        private async Task<RoomPhoto?> ResolvePhotoAsync(string entityType, int entityId, string photoType)
        {
            return entityType switch
            {
                "Room" => await _context.RoomPhotos
                    .Where(p => p.RoomId == entityId && p.PhotoType == photoType)
                    .OrderBy(p => p.SortOrder)
                    .FirstOrDefaultAsync(),
                _ => null,
            };
        }
    }
}
