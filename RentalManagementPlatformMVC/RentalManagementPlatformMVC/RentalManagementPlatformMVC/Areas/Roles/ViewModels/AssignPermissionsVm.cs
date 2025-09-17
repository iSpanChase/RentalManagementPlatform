using Microsoft.AspNetCore.Mvc.Rendering;

namespace RentalManagementPlatformMVC.Areas.Roles.ViewModels
{
	public class AssignPermissionsVm
	{
		public int RoleId { get; set; }
		public string RoleName { get; set; } = null!;

		public List<int> SelectedPermissionIds { get; set; } = new();
		public List<SelectListItem> AllPermissions { get; set; } = new();
	}
}
