using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Repositories
{
    public interface IBookingRepository
    {
        // 初始載入：取得所有訂單資料（支援分頁）
        Task<PagedResult<BookingDto>> GetAllBookingsAsync(int pageIndex, int pageSize);

        // 條件查詢：根據篩選條件查詢訂單
        //Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria);

        //// 取得單筆訂單明細（包含同行旅客）
        //Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId);
    }
}
