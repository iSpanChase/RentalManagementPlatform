using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// RefreshToken 資料存取實作：
    /// - 新增 token
    /// - 依 token 字串查詢
    /// - 查詢某使用者所有仍有效的 token
    /// </summary>
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public RefreshTokenRepository(RentalManagementPlatformSqlContext db) { _db = db; }

        /// <summary>
        /// 新增一筆 RefreshToken 並立即 SaveChanges
        /// </summary>
        public async Task AddAsync(RefreshToken token)
        {
            _db.RefreshTokens.Add(token);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 依 token 字串查詢單一 RefreshToken 紀錄
        /// </summary>
        public Task<RefreshToken?> GetAsync(string token) =>
            _db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

        /// <summary>
        /// 取得指定使用者目前仍「未撤銷且未過期」的 RefreshToken 清單：
        /// - 用於「全部裝置登出」等情境
        /// </summary>
        public Task<List<RefreshToken>> GetActiveByUserAsync(int userId, DateTime nowUtc) =>
            _db.RefreshTokens
               .Where(t => t.UserId == userId && !t.Revoked && t.ExpiresAt > nowUtc)
               .ToListAsync();

        /// <summary>
        /// 將暫存變更（例如 Revoked 標記）寫回資料庫
        /// </summary>
        public Task SaveChangesAsync() => _db.SaveChangesAsync();
    }
}
