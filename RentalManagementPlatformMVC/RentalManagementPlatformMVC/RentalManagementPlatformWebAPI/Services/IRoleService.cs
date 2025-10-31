using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
	public interface IRoleService
	{
		Task<List<RoleDto>> GetAllAsync();
		Task AssignUserAsync(int roleId, int userId);
		Task RevokeUserAsync(int roleId, int userId);
	}
}
