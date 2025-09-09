using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	public class UserIndexVm
	{
		public IReadOnlyList<UserListItemVm> Items { get; set; } = new List<UserListItemVm>();
		public string? Query { get; set; }
		public int CurrentPage { get; set; } = 1;
		public int PageSize { get; set; } = 10;
		public int Total { get; set; }
		public int TotalPages => Total <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);
	}
}
