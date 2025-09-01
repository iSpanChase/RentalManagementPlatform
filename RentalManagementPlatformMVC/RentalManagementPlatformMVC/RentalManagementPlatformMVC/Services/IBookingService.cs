using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
    public interface IBookingService
    {
        Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize);
        Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria);
        Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId);
    }
}   