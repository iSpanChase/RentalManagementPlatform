using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
    /// <summary>
    /// 權限（Permission）資料存取介面：
    /// - 提供查詢全部權限
    /// - 針對角色指派 / 移除權限
    /// - 依使用者取得權限代碼、依角色取得權限 Id
    /// </summary>
    public interface IPermissionRepository
    {
        /// <summary>
        /// 取得系統中所有權限實體清單
        /// </summary>
        Task<List<Permission>> GetAllAsync();

        /// <summary>
        /// 將一組權限 Id 指派給指定角色（通常會避免重複建立關聯）
        /// </summary>
        Task AssignToRoleAsync(int roleId, IEnumerable<int> permissionIds);

        /// <summary>
        /// 從指定角色移除一組權限 Id
        /// </summary>
        Task RemoveFromRoleAsync(int roleId, IEnumerable<int> permissionIds);

        /// <summary>
        /// 依使用者 UserId 取得此使用者所有「有效權限代碼」清單
        /// - 通常透過 UserRoles -> RolePermissions -> Permission 連動查詢
        /// - 給 PermissionClaimsTransformation 等地方使用
        /// </summary>
        Task<List<string>> GetCodesByUserIdAsync(int userId);

        /// <summary>
        /// 依角色 Id 取得此角色目前擁有的所有 PermissionId 清單
        /// - 通常用於前端勾選「角色擁有哪些權限」時帶預設值
        /// </summary>
        Task<List<int>> GetPermissionIdsByRoleIdAsync(int roleId);
    }
}
