using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
    public interface IBookingService
    {
		// 初始載入：取得所有訂單資料（支援分頁）
		Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據訂單ID取得訂單詳細資訊
		Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId);

		// 條件查詢：根據篩選條件查詢訂單
		Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria, int pageIndex,
		int pageSize);

	}
}