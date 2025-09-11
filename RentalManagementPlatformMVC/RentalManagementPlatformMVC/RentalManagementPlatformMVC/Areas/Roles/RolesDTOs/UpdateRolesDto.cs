namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public class UpdateRolesDto
	{
		public string RoleCode { get; set; } = null!;

		public string RoleName { get; set; } = null!;

		public string? Description { get; set; }
	}
}
