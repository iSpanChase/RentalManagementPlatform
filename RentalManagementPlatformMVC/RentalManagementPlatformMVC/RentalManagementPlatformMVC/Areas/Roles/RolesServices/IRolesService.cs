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
		Task<List<int>> GetUserIdsInRoleAsync(int roleId);
		Task SaveAssignUsersAsync(AssignUsersVm vm);
		Task<AssignPermissionsVm> GetAssignPermissionsVmAsync(int roleId);
		Task<List<int>> GetPermissionIdsInRoleAsync(int roleId);
		Task SaveAssignPermissionsAsync(AssignPermissionsVm vm);
		// 新增（GET 清單查詢，給新的 .cshtml 使用）
		Task<PagedResult<AssignableUserListItemDto>> QueryAssignableUsersAsync(AssignUsersQueryInput input);
		Task<PagedResult<AssignablePermissionListItemDto>> QueryAssignablePermissionsAsync(AssignPermissionsQueryInput input);

		// 新增（共用標題）
		Task<string> GetRoleNameAsync(int roleId);
	}
}
