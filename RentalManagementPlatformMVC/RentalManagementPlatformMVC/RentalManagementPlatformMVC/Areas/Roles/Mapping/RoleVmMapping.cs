using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.Areas.Roles.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Roles.Mapping
{
	public static class RoleVmMapping
	{
		public static CreateRolesDto ToCreateDto(this RoleFormVm vm) => new()
		{
			RoleCode = vm.RoleCode.Trim(),
			RoleName = vm.RoleName.Trim(),
			Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
		};

		public static UpdateRolesDto ToUpdateDto(this RoleFormVm vm) => new()
		{
			RoleCode = vm.RoleCode.Trim(),
			RoleName = vm.RoleName.Trim(),
			Description = string.IsNullOrWhiteSpace(vm.Description) ? null : vm.Description.Trim()
		};

		public static RoleFormVm ToFormVm(this RolesDetailDto dto, int roleId) => new()
		{
			RoleId = roleId,
			RoleCode = dto.RoleCode,
			RoleName = dto.RoleName,
			Description = dto.Description
		};
	}
}
