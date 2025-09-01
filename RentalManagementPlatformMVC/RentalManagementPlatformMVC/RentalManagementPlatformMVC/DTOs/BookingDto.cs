namespace RentalManagementPlatformMVC.DTOs
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public string MainGuestName { get; set; } // 主要旅客姓名
        public int GuestCount { get; set; }
    }
}
