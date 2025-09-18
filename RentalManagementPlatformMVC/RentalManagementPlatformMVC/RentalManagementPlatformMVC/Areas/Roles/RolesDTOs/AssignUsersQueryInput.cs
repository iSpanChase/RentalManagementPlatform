namespace RentalManagementPlatformMVC.Areas.Roles.RolesDTOs
{
	public class AssignUsersQueryInput
	{
		public int RoleId { get; set; }

		// 搜尋 / 排序 / 分頁
		public string? Keyword { get; set; }
		public string SortBy { get; set; } = "Name"; // 可用: Name, Username, Email
		public bool Desc { get; set; } = false;

		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 10;
	}
}
