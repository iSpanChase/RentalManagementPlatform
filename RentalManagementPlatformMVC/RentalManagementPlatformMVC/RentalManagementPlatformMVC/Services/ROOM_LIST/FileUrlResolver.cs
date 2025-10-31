using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Services.Interfaces;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Services
{
    public class FileUrlResolver : IFileUrlResolver
    {
        private readonly RentalManagementPlatformSqlContext _context;
        private readonly IMinioService _minioService;

        public FileUrlResolver(RentalManagementPlatformSqlContext context, IMinioService minioService)
        {
            _context = context;
            _minioService = minioService;
        }

        public async Task<string?> GetUrlAsync(string entityType, int entityId, string photoType)
        {
            string? objectKey = null;

            if (entityType == "Room")
            {
                objectKey = await _context.RoomPhotos
                    .Where(p => p.RoomId == entityId && p.PhotoType == photoType)
                    .OrderBy(p => p.SortOrder)
                    .Select(p => p.ObjectKey)
                    .FirstOrDefaultAsync();
            }

            return await GetPhotoUrlAsync(objectKey);
        }

        public async Task<string?> GetPhotoUrlAsync(string? objectKey)
        {
            if (string.IsNullOrWhiteSpace(objectKey))
            {
                return null;
            }

            return await _minioService.GetFileUrlAsync(objectKey);
        }
    }
}
