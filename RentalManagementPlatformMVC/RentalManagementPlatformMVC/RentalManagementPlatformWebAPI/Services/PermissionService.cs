using Microsoft.Extensions.Caching.Memory;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
    /// <summary>
    /// 權限服務實作：
    /// - 從 IPermissionRepository 取得權限資料並轉成 DTO
    /// - 針對角色指派 / 移除權限
    /// - 在角色權限異動時，清掉該角色相關使用者的 claims 快取
    ///   （避免使用者權限更改後仍用舊的快取資訊）
    /// </summary>
    public class PermissionService : IPermissionService
    {
        private readonly IPermissionRepository _repo;
        private readonly IRoleRepository _roles;          // 用來查該角色底下有哪些使用者
        private readonly IMemoryCache _cache;             // 用來清除使用者 claims 快取

        /// <summary>
        /// 透過 DI 注入：權限 Repo、角色 Repo 以及 MemoryCache
        /// </summary>
        public PermissionService(IPermissionRepository repo, IRoleRepository roles, IMemoryCache cache)
        {
            _repo = repo;
            _roles = roles;
            _cache = cache;
        }

        /// <summary>
        /// 取得系統中所有權限，並轉換為 PermissionDto 清單：
        /// - 若沒有設定名稱，預設使用權限代碼當作名稱
        /// - 若 Module 為空，預設為 "General"
        /// </summary>
        public async Task<List<PermissionDto>> GetAllAsync()
        {
            var list = await _repo.GetAllAsync();
            return list
                .Select(p => new PermissionDto(
                    p.PermissionId,
                    p.PermCode,
                    string.IsNullOrWhiteSpace(p.PermName) ? p.PermCode : p.PermName,
                    p.Module ?? "General"
                ))
                .ToList();
        }

        /// <summary>
        /// 將一組權限 Id 指派給指定角色，並在完成後清除該角色所有使用者的 claims 快取
        /// </summary>
        public async Task AssignAsync(int roleId, IEnumerable<int> permissionIds)
        {
            await _repo.AssignToRoleAsync(roleId, permissionIds);
            await InvalidateClaimsCacheForRole(roleId);   // 權限異動 → 清掉快取
        }

        /// <summary>
        /// 從指定角色移除一組權限 Id，並在完成後清除該角色所有使用者的 claims 快取
        /// </summary>
        public async Task RemoveAsync(int roleId, IEnumerable<int> permissionIds)
        {
            await _repo.RemoveFromRoleAsync(roleId, permissionIds);
            await InvalidateClaimsCacheForRole(roleId);   // 權限異動 → 清掉快取
        }

        /// <summary>
        /// ★ 重要：清除「此角色底下所有使用者」的 claims 快取
        /// - 先用角色 Repo 查出擁有此角色的所有 userId
        /// - 依每個 userId 移除對應的快取 key
        /// - key 格式需與 IClaimsTransformation / Auth 中實際使用的相同
        /// </summary>
        private async Task InvalidateClaimsCacheForRole(int roleId)
        {
            var userIds = await _roles.GetUserIdsByRoleIdAsync(roleId);
            foreach (var uid in userIds.Distinct())
            {
                _cache.Remove($"auth:claims:{uid}");      // ← key 需與 claims 快取實作一致
            }
        }

        /// <summary>
        /// 取得指定角色的所有權限 Id 清單（直接透過 Repo 查詢）
        /// </summary>
        public async Task<List<int>> GetIdsByRoleAsync(int roleId)
        {
            // repo 直接回傳該角色的 permission_id 清單即可
            return await _repo.GetPermissionIdsByRoleIdAsync(roleId);
        }
    }
}
