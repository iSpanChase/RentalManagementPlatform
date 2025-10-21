using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public interface IPermissionRepository
	{
		Task<List<Permission>> GetAllAsync();
		Task AssignToRoleAsync(int roleId, IEnumerable<int> permissionIds);
		Task RemoveFromRoleAsync(int roleId, IEnumerable<int> permissionIds);
		Task<List<string>> GetCodesByUserIdAsync(int userId);
	}
}
