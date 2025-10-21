using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
	public interface IPermissionService
	{
		Task<List<PermissionDto>> GetAllAsync();
		Task AssignAsync(int roleId, IEnumerable<int> permissionIds);
		Task RemoveAsync(int roleId, IEnumerable<int> permissionIds);
	}
}
