using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Services;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Areas.Booking.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Booking.Controllers
{
    [Area("Booking")]
    public class BookingHomeController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingHomeController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
        {
            var pagedResult = await _bookingService.GetPagedBookingsAsync(pageIndex, pageSize);

            var vm = new BookingIndexViewModel
            {
                Bookings = pagedResult.Items.Select(b => new BookingIndexRowViewModel
                {
                    OrderNumber = b.OrderNumber ?? "-",
                    GuestName = b.MainGuestName ?? "-",
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status ?? "-",
                    CreatedAt = b.CreatedAt,
                })
                .ToList(),

                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages
			};

            return View(vm);
        }
    }
}
