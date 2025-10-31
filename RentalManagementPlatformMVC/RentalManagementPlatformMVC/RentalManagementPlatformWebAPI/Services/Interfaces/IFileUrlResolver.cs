using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
    /// <summary>
    /// 定義檔案 URL 解析服務的介面。
    /// 負責根據實體類型、ID 和照片類型，從 MinIO 取得預簽章的檔案 URL。
    /// 這是從私有 MinIO 儲存桶安全地提供檔案的標準做法。
    /// </summary>
    public interface IFileUrlResolver
    {
        /// <summary>
        /// 根據實體資訊取得檔案的預簽章 URL。
        /// </summary>
        /// <param name="entityType">實體的類型 (例如: "Room", "User")。</param>
        /// <param name="entityId">實體的 ID。</param>
        /// <param name="photoType">照片的類型 (例如: "Cover", "Profile")。</param>
        /// <returns>檔案的預簽章 URL，如果找不到則為 null。</returns>
        Task<string?> GetUrlAsync(string entityType, int entityId, string photoType);
        Task<string?> GetPhotoUrlAsync(string? objectKey);
    }
}
