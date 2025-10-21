using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
	public class PermissionService : IPermissionService
	{
		private readonly IPermissionRepository _repo;
		public PermissionService(IPermissionRepository repo) { _repo = repo; }

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

		public Task AssignAsync(int roleId, IEnumerable<int> permissionIds) => _repo.AssignToRoleAsync(roleId, permissionIds);
		public Task RemoveAsync(int roleId, IEnumerable<int> permissionIds) => _repo.RemoveFromRoleAsync(roleId, permissionIds);
	}
}
