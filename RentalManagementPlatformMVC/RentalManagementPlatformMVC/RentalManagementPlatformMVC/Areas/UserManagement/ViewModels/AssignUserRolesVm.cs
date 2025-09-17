namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	public class AssignUserRolesVm
	{
		public int UserId { get; set; }
		public string Username { get; set; } = "";
		public string? Email { get; set; }
		public string Name { get; set; } = "";

		// 供畫面顯示的所有角色
		public List<RoleCheckItem> Roles { get; set; } = new();

		// 用來接回勾選結果（POST）
		public int[] SelectedRoleIds { get; set; } = Array.Empty<int>();
	}
	public class RoleCheckItem
	{
		public int RoleId { get; set; }
		public string RoleCode { get; set; } = "";
		public string RoleName { get; set; } = "";
		public bool Checked { get; set; }
	}
}
