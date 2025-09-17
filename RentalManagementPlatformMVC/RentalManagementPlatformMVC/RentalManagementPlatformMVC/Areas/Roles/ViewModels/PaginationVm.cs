using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Roles.ViewModels
{
	public class PaginationVm
	{
		/// <summary>目前頁次（1 起算）。</summary>
		public int Page { get; init; } = 1;
		/// <summary>每頁筆數。</summary>
		public int PageSize { get; init; } = 10;
		/// <summary>總筆數。</summary>
		public int Total { get; init; }
		/// <summary>總頁數（依總筆數動態計算）。</summary>
		public int TotalPages => Total <= 0 ? 1 : (int)Math.Ceiling(Total / (double)PageSize);
	}
}
