namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	public class UserFilterVm
	{
		// 關鍵字：帳號/Email/姓名
		public string? Keyword { get; set; }

		// 性別：M/F/空(全部)
		public string? Gender { get; set; }

		// 是否已驗證：null=全部、true=已驗證、false=未驗證
		public bool? Isverified { get; set; }

		// 建立時間區間（yyyy-MM-dd）
		public DateTime? CreatedFrom { get; set; }
		public DateTime? CreatedTo { get; set; }

		// 你已經有的排序/分頁
		public string? SortBy { get; set; } = "createdAt"; // id/username/name/email/createdAt
		public string? SortDir { get; set; } = "desc";     // asc/desc
		public int Page { get; set; } = 1;
		public int PageSize { get; set; } = 20;
	}
}
