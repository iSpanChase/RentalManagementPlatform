namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs
{
	public class PermissionDetailDto
	{
		public int PermissionId { get; set; }
		public string PermCode { get; set; } = default!;
		public string PermName { get; set; } = default!;
		public string Module { get; set; } = default!;
		public string Action { get; set; } = default!;
		public string? Description { get; set; }
		public DateTime CreatedAt { get; set; }
		public DateTime UpdatedAt { get; set; }
	}
}
