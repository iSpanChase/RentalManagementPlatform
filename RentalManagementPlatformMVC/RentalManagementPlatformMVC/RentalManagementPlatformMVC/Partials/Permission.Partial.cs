namespace RentalManagementPlatformMVC.Models
{
	public partial class Permission
	{
		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
	}
}
