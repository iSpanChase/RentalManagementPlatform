using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 角色讀寫與指派/回收使用者的資料存取介面
    /// - 以 Id 或角色代碼取得角色實體
    /// - 查詢全部角色
    /// - 對使用者指派 / 移除角色
    /// - 檢查角色是否仍被任何使用者使用
    /// - 依使用者取得角色代碼、依角色取得使用者 Id 清單
    /// </summary>
    public interface IRoleRepository
    {
        /// <summary>以主鍵取得角色</summary>
        Task<Role?> GetByIdAsync(int roleId);

        /// <summary>以角色代碼(如 ADMIN/TENANT)取得角色</summary>
        Task<Role?> GetByCodeAsync(string roleCode);

        /// <summary>取得全部角色（依 RoleCode 排序）</summary>
        Task<List<Role>> GetAllAsync();

        /// <summary>把指定使用者指派到角色（若已存在則忽略）</summary>
        Task AssignUserAsync(int roleId, int userId);

        /// <summary>把指定使用者從角色移除（若不存在則忽略）</summary>
        Task RevokeUserAsync(int roleId, int userId);

        /// <summary>該角色是否仍被任何使用者使用（用於刪除前檢查）</summary>
        Task<bool> HasUsersAsync(int roleId);

        /// <summary>
        /// 依使用者 UserId 取得此使用者所有角色代碼（RoleCode）清單
        /// - 給 AuthService / ClaimsTransformation 等地方填入角色 claims 使用
        /// </summary>
        Task<List<string>> GetCodesByUserIdAsync(int userId);

        /// <summary>
        /// 依角色 Id 取得所有擁有此角色的使用者 UserId 清單
        /// - 給 PermissionService 在權限異動時，清除相關使用者快取使用
        /// </summary>
        Task<List<int>> GetUserIdsByRoleIdAsync(int roleId);
    }
}
