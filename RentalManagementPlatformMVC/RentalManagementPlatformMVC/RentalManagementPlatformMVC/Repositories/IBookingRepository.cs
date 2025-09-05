using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Repositories
{
    public interface IBookingRepository
    {
        // 初始載入：取得所有訂單資料（支援分頁）
        Task<(IEnumerable<Booking>, int)> GetPagedBookingsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據訂單ID取得訂單詳細資訊
		Task<Booking?> GetByIdAsync(int bookingId);

		// 動態條件查詢：根據篩選條件查詢訂單
		Task<(IEnumerable<Booking>, int)> SearchAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
