using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
	public class RoleService : IRoleService
	{
		private readonly IRoleRepository _repo;
		public RoleService(IRoleRepository repo) { _repo = repo; }

		public async Task<List<RoleDto>> GetAllAsync()
		{
			var roles = await _repo.GetAllAsync();
			return roles.Select(r => new RoleDto(
				r.RoleId,
				string.IsNullOrWhiteSpace(r.RoleName) ? r.RoleCode : r.RoleName,
				r.RoleCode
			)).ToList();
		}
		public Task AssignUserAsync(int roleId, int userId) => _repo.AssignUserAsync(roleId, userId);
		public Task RevokeUserAsync(int roleId, int userId) => _repo.RevokeUserAsync(roleId, userId);
	}
}
