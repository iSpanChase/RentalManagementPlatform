using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Services.Interfaces;
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
            string? objectKey = null;

            if (entityType == "Room")
            {
                objectKey = await _context.RoomPhotos
                                    .Where(p => p.RoomId == entityId && p.PhotoType == photoType)
                                    .OrderBy(p => p.SortOrder)
                                    .Select(p => p.ObjectKey)
                                    .FirstOrDefaultAsync();
            }

            if (string.IsNullOrEmpty(objectKey))
            {
                return null;
            }

            return await _minioService.GetFileUrlAsync(objectKey);
        }
    }
}