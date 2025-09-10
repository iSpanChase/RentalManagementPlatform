using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Areas.UserManagement.ViewModels
{
	/// <summary>
	/// 使用者清單頁的 ViewModel（含分頁資訊）。
	/// </summary>
	public class UserIndexVm
	{
		/// <summary>清單資料。</summary>
		public IReadOnlyList<UserListItemVm> Items { get; set; } = new List<UserListItemVm>();
		/// <summary>關鍵字。</summary>
		public string? Query { get; set; }
		/// <summary>目前頁次（1 起算）。</summary>
		public int CurrentPage { get; set; } = 1;
		/// <summary>每頁筆數。</summary>
		public int PageSize { get; set; } = 10;
		/// <summary>總筆數。</summary>
		public int Total { get; set; }
		/// <summary>總頁數（依總筆數動態計算）。</summary>
		public int TotalPages => Total <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);
	}
}
