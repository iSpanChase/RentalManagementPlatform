namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public record AssignableUserListItemDto(
		int UserId, string Username, string Name, string Email, bool Selected);
}
