using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 角色資料存取實作：
    /// - 角色查詢（依 Id / Code / 全部）
    /// - 對使用者指派 / 收回角色
    /// - 查詢角色是否被使用、以及角色下有哪些 userId
    /// </summary>
    public class RoleRepository : IRoleRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public RoleRepository(RentalManagementPlatformSqlContext db) => _db = db;

        /// <summary>
        /// 依 RoleId 取得單一角色
        /// </summary>
        public Task<Role?> GetByIdAsync(int roleId) =>
            _db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);

        /// <summary>
        /// 依 RoleCode 取得單一角色（例如 "ADMIN", "TENANT"...）
        /// </summary>
        public Task<Role?> GetByCodeAsync(string roleCode) =>
            _db.Roles.FirstOrDefaultAsync(r => r.RoleCode == roleCode);

        /// <summary>
        /// 取得全部角色，依 RoleCode 排序
        /// </summary>
        public Task<List<Role>> GetAllAsync() =>
            _db.Roles.OrderBy(r => r.RoleCode).ToListAsync();

        /// <summary>
        /// 將指定使用者指派到某個角色：
        /// - 若該 user 已有此角色則不重複新增
        /// </summary>
        public async Task AssignUserAsync(int roleId, int userId)
        {
            var exists = await _db.UserRoles
                .AnyAsync(x => x.RoleId == roleId && x.UserId == userId);
            if (!exists)
            {
                _db.UserRoles.Add(new UserRole { RoleId = roleId, UserId = userId });
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 從指定使用者收回某個角色：
        /// - 若找不到對應 UserRole 則不做事
        /// </summary>
        public async Task RevokeUserAsync(int roleId, int userId)
        {
            var ur = await _db.UserRoles
                .FirstOrDefaultAsync(x => x.RoleId == roleId && x.UserId == userId);
            if (ur != null)
            {
                _db.UserRoles.Remove(ur);
                await _db.SaveChangesAsync();
            }
        }

        /// <summary>
        /// 檢查某角色是否仍被任何使用者使用：
        /// - 通常用在刪除角色前的防呆
        /// </summary>
        public Task<bool> HasUsersAsync(int roleId) =>
            _db.UserRoles.AnyAsync(x => x.RoleId == roleId);

        /// <summary>
        /// 依 UserId 取得該使用者所有角色代碼（RoleCode）清單
        /// </summary>
        public Task<List<string>> GetCodesByUserIdAsync(int userId) => _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.RoleCode)
            .Distinct()
            .ToListAsync();

        /// <summary>
        /// 依 RoleId 取得所有擁有此角色的 UserId 清單（去重）
        /// </summary>
        public Task<List<int>> GetUserIdsByRoleIdAsync(int roleId) => _db.UserRoles
            .Where(ur => ur.RoleId == roleId)
            .Select(ur => ur.UserId)
            .Distinct()
            .ToListAsync();
    }
}
