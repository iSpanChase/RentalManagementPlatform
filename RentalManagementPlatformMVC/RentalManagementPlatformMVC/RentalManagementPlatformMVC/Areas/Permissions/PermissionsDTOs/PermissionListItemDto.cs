namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs
{
	public record PermissionListItemDto(
		int PermissionId,
		string PermCode,
		string PermName,
		string Module,
		string Action,
		string? Description,
		DateTime CreatedAt,
		DateTime UpdatedAt
		);
}
