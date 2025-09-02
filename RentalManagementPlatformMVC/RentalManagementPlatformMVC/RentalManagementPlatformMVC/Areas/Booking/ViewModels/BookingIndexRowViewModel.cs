namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
    public class BookingIndexRowViewModel
    {
        public string OrderNumber { get; set; }
        public string GuestName { get; set; }   // 主入住人或第一位
        public string RoomName { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
