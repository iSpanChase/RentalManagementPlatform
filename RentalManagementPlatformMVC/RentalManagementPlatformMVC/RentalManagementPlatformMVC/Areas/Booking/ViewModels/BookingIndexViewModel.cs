using System.Collections.Generic;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
    public class BookingIndexViewModel
    {
        public List<BookingIndexRowViewModel> Bookings { get; set; } = new();
        public int PageIndex { get; set; }
		public int PageSize { get; set; }
        public int TotalPages { get; set; }
	}
}
