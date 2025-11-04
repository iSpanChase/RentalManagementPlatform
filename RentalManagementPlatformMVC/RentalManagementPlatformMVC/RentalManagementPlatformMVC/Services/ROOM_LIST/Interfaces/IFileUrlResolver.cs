using System.Threading.Tasks;

namespace RentalManagementPlatformMVC.Services.Interfaces
{
    /// <summary>
    /// 一個通用的服務，用來解析實體的檔案 URL。
    /// </summary>
    public interface IFileUrlResolver
    {
        /// <summary>
        /// 根據實體類型、ID 和照片類型，取得檔案的公開 URL。
        /// </summary>
        /// <param name="entityType">實體類型，例如 "Room"。</param>
        /// <param name="entityId">實體的 ID，例如房源 ID。</param>
        /// <param name="photoType">照片類型，例如 "Cover" 或 "Gallery"。</param>
        /// <returns>一個有時效性的預簽章 URL，如果找不到則為 null。</returns>
        Task<string?> GetUrlAsync(string entityType, int entityId, string photoType);
        Task<string?> GetPhotoUrlAsync(string? objectKey);
    }
}
