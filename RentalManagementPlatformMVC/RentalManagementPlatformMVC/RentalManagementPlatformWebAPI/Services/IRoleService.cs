using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 角色（Role）相關服務介面
    /// - 提供查詢所有角色
    /// - 對使用者指派 / 收回角色
    /// </summary>
    public interface IRoleService
    {
        /// <summary>
        /// 取得系統中所有角色清單（用 RoleDto 表示）
        /// </summary>
        Task<List<RoleDto>> GetAllAsync();

        /// <summary>
        /// 將指定角色指派給某個使用者
        /// </summary>
        Task AssignUserAsync(int roleId, int userId);

        /// <summary>
        /// 收回某個使用者的指定角色
        /// </summary>
        Task RevokeUserAsync(int roleId, int userId);
    }
}
