namespace RentalManagementPlatformMVC.Areas.Permissions.ViewModels
{
	public record SimpleRoleVm(int RoleId, string RoleCode, string RoleName);
	public class PermissionUsedByRolesVm
	{
		public int PermissionId { get; set; }
		public string PermCode { get; set; } = default!;
		public string PermName { get; set; } = default!;
		public string Module { get; set; } = default!;
		public string Action { get; set; } = default!;
		public List<SimpleRoleVm> Roles { get; set; } = new();
	}
}
