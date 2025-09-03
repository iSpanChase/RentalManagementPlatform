using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
    public interface IBookingService
    {
		// 初始載入：取得所有訂單資料（支援分頁）
		Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageNumber, int pageSize);

		// 詳細頁面：根據訂單ID取得訂單詳細資訊
		Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId);

        //Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria);

    }
}