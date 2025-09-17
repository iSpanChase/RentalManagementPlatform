using RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs;
using RentalManagementPlatformMVC.Areas.Permissions.ViewModels;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public interface IPermissionsService
	{
		Task<PagedResult<PermissionListItemDto>> QueryAsync(PermissionQueryInput input);
		Task<int> CreateAsync(CreatePermissionDto dto);
		Task UpdateAsync(int id, UpdatePermissionDto dto);
		Task DeleteAsync(int id);
		Task<PermissionDetailDto?> GetDetailAsync(int id);

		// 唯讀：查此權限被哪些角色使用（不做指派，避免重工）
		Task<PermissionUsedByRolesVm> GetUsedByRolesVmAsync(int permissionId);
	}
}
