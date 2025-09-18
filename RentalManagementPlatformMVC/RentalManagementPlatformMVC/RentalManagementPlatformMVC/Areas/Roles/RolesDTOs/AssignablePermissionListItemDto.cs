namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public record AssignablePermissionListItemDto(
		int PermissionId, string Module, string Action, string Name, string Code, bool Selected);
}
