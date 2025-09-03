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

        public async Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize)
        {
            return await _bookingRepository.GetAllBookingsAsync(pageIndex, pageSize);
        }

        //public async Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria)
        //{
        //    return await _bookingRepository.SearchBookingsAsync(criteria);
        //}

        //public async Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId)
        //{
        //    return await _bookingRepository.GetBookingDetailByIdAsync(bookingId);
        //}
    }
}