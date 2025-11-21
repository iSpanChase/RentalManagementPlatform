using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// RefreshToken 資料存取介面：
    /// - 新增 RefreshToken 記錄
    /// - 依 token 字串查詢
    /// - 依 userId 查詢目前仍有效的 RefreshToken（用於全部登出）
    /// - 儲存變更（例如標記 Revoked）
    /// </summary>
    public interface IRefreshTokenRepository
    {
        /// <summary>
        /// 新增一筆 RefreshToken（通常在登入或刷新 token 時建立）
        /// </summary>
        Task AddAsync(RefreshToken token);

        /// <summary>
        /// 依 token 字串查詢單一 RefreshToken（用於 Refresh 流程驗證）
        /// </summary>
        Task<RefreshToken?> GetAsync(string token);

        /// <summary>
        /// 取得某個使用者目前仍「未過期且未被撤銷」的 RefreshToken 清單
        /// - nowUtc 由外部傳入目前時間（UTC）
        /// - 通常在「全部裝置登出」時使用
        /// </summary>
        Task<List<RefreshToken>> GetActiveByUserAsync(int userId, DateTime nowUtc);

        /// <summary>
        /// 將針對 RefreshToken 所做的變更（新增/更新/撤銷）寫回資料庫
        /// </summary>
        Task SaveChangesAsync();
    }
}
