using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.DTOs.Bookings;
using RentalManagementPlatform.Common.Pagination;

namespace RentalManagementPlatformMVC.Repositories.Bookings
{
	public interface IBookingRepository
	{
		// 初始載入：取得所有Booking資料（支援分頁）
		Task<PagedResult<Booking>> GetPagedBookingsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據Booking ID取得訂單詳細資訊
		Task<Booking?> GetBookingDetailByIdAsync(int bookingId);

		// 動態條件查詢：根據篩選條件查詢Booking
		Task<(IEnumerable<Booking>, int)> SearchBookingsAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
