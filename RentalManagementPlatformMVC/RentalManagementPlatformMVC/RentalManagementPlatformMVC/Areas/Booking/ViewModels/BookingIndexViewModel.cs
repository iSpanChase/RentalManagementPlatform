using System.Collections.Generic;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
	/// <summary>
	/// 訂單管理頁面的 ViewModel，封裝了分頁後的訂單清單與分頁資訊，提供前端 Razor View 使用。
	/// </summary>
	public class BookingIndexViewModel
    {
		/// 目前頁面中的訂單清單，每筆訂單對應一個 <see cref="BookingIndexRowViewModel"/>。
		public List<BookingIndexRowViewModel> Bookings { get; set; } = new();
        public int PageIndex { get; set; }
		public int PageSize { get; set; }
        public int TotalPages { get; set; }
		public int TotalCount { get; set; }
		public BookingSearchCriteriaDto Criteria { get; set; } = new();
	}
}
