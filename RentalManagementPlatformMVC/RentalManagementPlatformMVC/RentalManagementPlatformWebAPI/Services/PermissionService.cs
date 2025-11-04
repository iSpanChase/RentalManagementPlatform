using Microsoft.Extensions.Caching.Memory;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
	public class PermissionService : IPermissionService
	{
		private readonly IPermissionRepository _repo;
		private readonly IRoleRepository _roles;          // ★ 新增
		private readonly IMemoryCache _cache;             // ★ 新增
		public PermissionService(IPermissionRepository repo, IRoleRepository roles, IMemoryCache cache)
		{
			_repo = repo;
			_roles = roles;
			_cache = cache;
		}

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

		public async Task AssignAsync(int roleId, IEnumerable<int> permissionIds)
		{
			await _repo.AssignToRoleAsync(roleId, permissionIds);
			await InvalidateClaimsCacheForRole(roleId);   // ★ 新增
		}
			
		public async Task RemoveAsync(int roleId, IEnumerable<int> permissionIds)
		{
			await _repo.RemoveFromRoleAsync(roleId, permissionIds);
			await InvalidateClaimsCacheForRole(roleId);   // ★ 新增
		}
		// ★ 新增：清掉「此角色所有使用者」的 claims 快取
		private async Task InvalidateClaimsCacheForRole(int roleId)
		{
			var userIds = await _roles.GetUserIdsByRoleIdAsync(roleId);
			foreach (var uid in userIds.Distinct())
			{
				_cache.Remove($"auth:claims:{uid}");      // ← 這個 key 要與 IClaimsTransformation 用的一致
			}
		}
		public async Task<List<int>> GetIdsByRoleAsync(int roleId)
		{
			// repo 直接回傳該角色的 permission_id 清單即可
			return await _repo.GetPermissionIdsByRoleIdAsync(roleId);
		}

	}
}
