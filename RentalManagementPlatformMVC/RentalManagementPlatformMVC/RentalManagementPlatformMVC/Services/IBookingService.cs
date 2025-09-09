using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatform.Common.Pagination;

namespace RentalManagementPlatformMVC.Services
{
    public interface IBookingService
    {
		// 初始載入：取得所有Booking資料（支援分頁）
		Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據Booking ID取得訂單詳細資訊
		Task<BookingDetailDto?> GetBookingDetailByIdAsync(int bookingId);

		// 動態條件查詢：根據篩選條件查詢Booking
		Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}