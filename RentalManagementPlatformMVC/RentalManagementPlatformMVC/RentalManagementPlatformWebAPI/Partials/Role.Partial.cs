namespace RentalManagementPlatformMVC.Models
{
	public partial class Role
	{
		public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
		public ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
	}
}
