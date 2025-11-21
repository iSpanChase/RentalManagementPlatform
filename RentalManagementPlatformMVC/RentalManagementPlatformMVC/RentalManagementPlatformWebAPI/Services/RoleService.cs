using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 角色服務實作：
    /// - 取得系統中所有角色並轉成 RoleDto
    /// - 負責對使用者指派 / 收回角色（委派給 IRoleRepository）
    /// </summary>
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _repo;

        /// <summary>
        /// 透過 DI 注入角色 Repository
        /// </summary>
        public RoleService(IRoleRepository repo) { _repo = repo; }

        /// <summary>
        /// 取得系統中所有角色並轉換成 RoleDto 清單：
        /// - 若 RoleName 為空，則顯示名稱使用 RoleCode
        /// - RoleDto 結構：Id, 顯示名稱, 代碼
        /// </summary>
        public async Task<List<RoleDto>> GetAllAsync()
        {
            var roles = await _repo.GetAllAsync();
            return roles.Select(r => new RoleDto(
                r.RoleId,
                string.IsNullOrWhiteSpace(r.RoleName) ? r.RoleCode : r.RoleName,
                r.RoleCode
            )).ToList();
        }

        /// <summary>
        /// 將指定角色指派給某個使用者（直接呼叫 Repo）
        /// </summary>
        public Task AssignUserAsync(int roleId, int userId) => _repo.AssignUserAsync(roleId, userId);

        /// <summary>
        /// 收回某個使用者的指定角色（直接呼叫 Repo）
        /// </summary>
        public Task RevokeUserAsync(int roleId, int userId) => _repo.RevokeUserAsync(roleId, userId);
    }
}
