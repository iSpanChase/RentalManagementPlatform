using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs
{
	public record UserListItemDto(
		int UserId, string Username, string Email, string Name, DateTime CreatedAt);
}
