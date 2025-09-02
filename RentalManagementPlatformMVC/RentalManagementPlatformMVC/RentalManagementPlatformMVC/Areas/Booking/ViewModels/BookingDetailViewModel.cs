using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
    public class BookingDetailViewModel
    {
        public string OrderNumber { get; set; }
        public string RoomName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? CommissionRateSnapshot { get; set; }
        public int? PointsEarned { get; set; }
        public int? PointsRedeemed { get; set; }
        public string CouponCode { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        public PagedResult<BookingGuestViewModel> Guests { get; set; }
    }
}
