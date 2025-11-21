using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 權限（Permission）相關服務介面
    /// - 提供查詢所有權限
    /// - 對角色指派 / 移除權限
    /// - 查詢角色目前擁有的權限 Id 清單
    /// </summary>
    public interface IPermissionService
    {
        /// <summary>
        /// 取得系統中所有權限的清單（用 PermissionDto 表示）
        /// </summary>
        Task<List<PermissionDto>> GetAllAsync();

        /// <summary>
        /// 將一組權限 Id 指派給指定角色
        /// </summary>
        Task AssignAsync(int roleId, IEnumerable<int> permissionIds);

        /// <summary>
        /// 從指定角色移除一組權限 Id
        /// </summary>
        Task RemoveAsync(int roleId, IEnumerable<int> permissionIds);

        /// <summary>
        /// 取得指定角色目前擁有的所有權限 Id 清單
        /// （通常用於前端勾選「角色擁有哪些權限」時預設勾選）
        /// </summary>
        Task<List<int>> GetIdsByRoleAsync(int roleId);
    }
}
