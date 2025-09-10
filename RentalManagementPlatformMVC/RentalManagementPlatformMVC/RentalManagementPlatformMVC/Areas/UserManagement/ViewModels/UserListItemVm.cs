using System;

namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	public class UserListItemVm
	{
		public int UserId { get; set; }
		public string Username { get; set; } = null!;
		public string Name { get; set; } = null!;
		public string Email { get; set; } = null!;
		public DateTime CreatedAt { get; set; }
	}
}
