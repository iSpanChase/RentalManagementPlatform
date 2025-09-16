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

                        // TODO: 當您在 RoomPhoto 資料表和 C# 類別中加入 PhotoType 欄位後，請取消此區塊的註解。
            if (entityType == "Room")
            {
                objectKey = await _context.RoomPhotos
                                    .Where(p => p.RoomId == entityId && p.PhotoType == photoType)
                                    .OrderBy(p => p.SortOrder)
                                    .Select(p => p.ObjectKey)
                                    .FirstOrDefaultAsync();
            }

            // else if (entityType == "User") { ... 未來可擴充 ... }


            if (string.IsNullOrEmpty(objectKey))
            {
                return null; // 目前因為邏輯被註解，所以會直接回傳 null
            }

            return await _minioService.GetFileUrlAsync(objectKey);
        }
    }
}
