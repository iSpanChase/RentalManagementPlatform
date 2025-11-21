using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 使用者（User）資料存取介面：
    /// - 依 Email / Username / 第三方 Provider 查詢
    /// - 依 Id 取得單一使用者
    /// - 新增使用者
    /// - 儲存變更
    /// - 提供 IQueryable 供高階服務做進階查詢
    /// </summary>
    public interface IUserRepository
    {
        /// <summary>
        /// 依 Email 查詢使用者（Email 為唯一值時可當登入帳號使用）
        /// </summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>
        /// 依 Username 查詢使用者（用於前台以使用者名稱顯示 / 查詢）
        /// </summary>
        Task<User?> GetByUsernameAsync(string username);

        /// <summary>
        /// 依第三方登入 Provider + Subject（ProviderUserId）查詢使用者
        /// - 給 Google / LINE 等第三方登入情境使用
        /// </summary>
        Task<User?> GetByProviderAsync(string provider, string subject);

        /// <summary>
        /// 依 UserId 取得單一使用者實體
        /// </summary>
        Task<User?> GetByIdAsync(int userId);

        /// <summary>
        /// 新增一個使用者實體（尚未 SaveChanges）
        /// </summary>
        Task AddAsync(User user);

        /// <summary>
        /// 將對 User 的新增/修改等變更寫回資料庫
        /// </summary>
        Task SaveChangesAsync();

        /// <summary>
        /// 回傳 IQueryable<User>，讓上層服務可自行組合查詢條件 / 投影
        /// - 注意：實作方通常會回傳 DbSet&lt;User&gt;.AsQueryable()
        /// </summary>
        IQueryable<User> Query();
    }
}
