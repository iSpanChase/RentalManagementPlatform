using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Repositories;

namespace RentalManagementPlatformMVC.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

		/// <summary>
		/// 初始載入：取得所有訂單資料（支援分頁）
		/// </summary>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public async Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize)
        {
            return await _bookingRepository.GetAllBookingsAsync(pageIndex, pageSize);
        }

		/// <summary>
		/// 詳細頁面：根據訂單ID取得訂單詳細資訊
		/// </summary>
		/// <param name="bookingId"></param>
		/// <returns></returns>
		public async Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId)
        {
            return await _bookingRepository.GetBookingDetailByIdAsync(bookingId);
        }


        //public async Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria)
        //{
        //    return await _bookingRepository.SearchBookingsAsync(criteria);
        //}

    }
}