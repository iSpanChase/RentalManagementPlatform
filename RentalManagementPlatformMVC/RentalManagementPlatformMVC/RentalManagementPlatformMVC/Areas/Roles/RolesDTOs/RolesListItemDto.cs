namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public record RolesListItemDto(
		int RoleId, string RoleCode, string RoleName, DateTime CreatedAt);
}
