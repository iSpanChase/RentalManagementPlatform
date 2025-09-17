using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.Areas.Roles.ViewModels;


namespace RentalManagementPlatformMVC.Areas.Roles.RolesServices
{
	public interface IRolesService
	{
		Task<PagedResult<RolesListItemDto>> QueryAsync(RoleQueryInput input);
		Task<int> CreateAsync(CreateRolesDto dto);
		Task UpdateAsync(int roleId, UpdateRolesDto dto);
		Task DeleteAsync(int roleId);
		Task<RolesDetailDto?> GetDetailAsync(int roleId);

		Task<AssignUsersVm> GetAssignUsersVmAsync(int roleId);
		Task SaveAssignUsersAsync(AssignUsersVm vm);
		Task<AssignPermissionsVm> GetAssignPermissionsVmAsync(int roleId);
		Task SaveAssignPermissionsAsync(AssignPermissionsVm vm);
	}
}
