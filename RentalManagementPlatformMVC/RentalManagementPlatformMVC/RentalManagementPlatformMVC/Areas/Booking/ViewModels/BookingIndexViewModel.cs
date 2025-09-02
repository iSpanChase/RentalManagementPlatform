using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
    public class BookingIndexViewModel
    {
        public PagedResult<BookingIndexRowViewModel> PagedBookings { get; set; }
        public PaginationInfo Pagination { get; set; }
    }
}
