using System.Linq;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 權限資料存取實作：
    /// - 查詢全部權限
    /// - 對角色指派 / 移除權限
    /// - 依使用者 / 角色取得權限資訊
    /// </summary>
    public class PermissionRepository : IPermissionRepository
    {
        private readonly RentalManagementPlatformSqlContext _db;
        public PermissionRepository(RentalManagementPlatformSqlContext db) { _db = db; }

        /// <summary>
        /// 取得全部 Permission，依 PermCode 排序
        /// </summary>
        public Task<List<Permission>> GetAllAsync() => _db.Permissions.OrderBy(p => p.PermCode).ToListAsync();

        /// <summary>
        /// 將一組權限 Id 指派給指定角色：
        /// - 先把傳入的 permissionIds 去重
        /// - 查出已存在於 RolePermissions 的關聯
        /// - 只對尚未存在的 Id 建立 RolePermission 記錄
        /// </summary>
        public async Task AssignToRoleAsync(int roleId, IEnumerable<int> permissionIds)
        {
            var toAdd = permissionIds.Distinct().ToList();
            var existing = await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId && toAdd.Contains(rp.PermissionId))
                .Select(rp => rp.PermissionId)
                .ToListAsync();

            foreach (var pid in toAdd.Except(existing))
                _db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 從指定角色移除一組權限：
        /// - 找出 RolePermissions 中符合 roleId + permissionIds 的所有項目
        /// - 批次移除後 SaveChanges
        /// </summary>
        public async Task RemoveFromRoleAsync(int roleId, IEnumerable<int> permissionIds)
        {
            var items = await _db.RolePermissions
                .Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
                .ToListAsync();

            _db.RolePermissions.RemoveRange(items);
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// 依 UserId 取得此使用者所有權限代碼：
        /// - 由 UserRoles → Role → RolePermissions → Permission.PermCode
        /// - 使用 Distinct 去除重複
        /// </summary>
        public Task<List<string>> GetCodesByUserIdAsync(int userId) => _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermCode))
            .Distinct()
            .ToListAsync();

        /// <summary>
        /// 依角色 Id 取得該角色目前擁有的所有 PermissionId 清單
        /// </summary>
        public Task<List<int>> GetPermissionIdsByRoleIdAsync(int roleId) => _db.RolePermissions
           .Where(rp => rp.RoleId == roleId)
           .Select(rp => rp.PermissionId)
           .ToListAsync();
    }
}
