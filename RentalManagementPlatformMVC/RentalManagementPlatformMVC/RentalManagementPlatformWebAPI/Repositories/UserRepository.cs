using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 使用者資料存取實作：
    /// - 依 Id / Email / Username / Provider 查詢
    /// - 新增使用者與 SaveChanges
    /// - 提供 IQueryable 給上層做彈性查詢
    /// </summary>
    public class UserRepository : IUserRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public UserRepository(RentalManagementPlatformSqlContext db) { _db = db; }

        /// <summary>
        /// 依 UserId 取得單一 User
        /// </summary>
        public Task<User?> GetByIdAsync(int userId) =>
            _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

        /// <summary>
        /// 依 Email 取得 User（Email 通常為唯一值）
        /// </summary>
        public Task<User?> GetByEmailAsync(string email) =>
            _db.Users.FirstOrDefaultAsync(u => u.Email == email);

        /// <summary>
        /// 依 Username 取得 User
        /// </summary>
        public Task<User?> GetByUsernameAsync(string username) =>
            _db.Users.FirstOrDefaultAsync(u => u.Username == username);

        /// <summary>
        /// 依第三方登入 Provider + Subject 查詢 User：
        /// - provider：例如 "Google", "Line"
        /// - subject：對應 Provider 提供的 user id
        /// </summary>
        public Task<User?> GetByProviderAsync(string provider, string subject) =>
            _db.Users.FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderSubject == subject);

        /// <summary>
        /// 新增一個 User，並立即 SaveChanges
        /// </summary>
        public async Task AddAsync(User user)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 將目前對 User 的變更寫回資料庫
        /// </summary>
        public Task SaveChangesAsync() => _db.SaveChangesAsync();

        /// <summary>
        /// 回傳 IQueryable&lt;User&gt;，讓上層可自行加 Where / Select 等組合查詢
        /// </summary>
        public IQueryable<User> Query() => _db.Users.AsQueryable();
    }
}
