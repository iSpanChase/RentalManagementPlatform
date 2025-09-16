namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs
{
	public class CreatePermissionDto
	{
		public string PermCode { get; set; } = default!;
		public string PermName { get; set; } = default!;
		public string Module { get; set; } = default!;
		public string Action { get; set; } = default!;
		public string? Description { get; set; }
	}
}
