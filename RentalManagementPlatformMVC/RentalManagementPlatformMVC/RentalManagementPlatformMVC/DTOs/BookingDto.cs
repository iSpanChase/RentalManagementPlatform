namespace RentalManagementPlatformMVC.DTOs
{
    /// <summary>
    /// 訂單資料傳輸物件，用於封裝訂單的基礎資訊，提供前端顯示或跨層傳遞使用。
    /// </summary>
    public class BookingDto
    {
        public int BookingId { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // 關聯資料
        public string? GuestName { get; set; }
        public int? GuestCount { get; set; }
        public string? Room { get; set; }
    }
}
