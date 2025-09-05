namespace RentalManagementPlatformMVC.DTOs
{
	/// <summary>
	/// 每筆訂單的詳細資料傳輸物件，包含訂單的完整資訊及其關聯資料。
	/// </summary>
	public class BookingDetailDto
    {
        public int BookingId { get; set; }
        public string? OrderNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? CommissionRateSnapshot { get; set; }
        public int? PointsEarned { get; set; }
        public int? PointsRedeemed { get; set; }
        public string? Status { get; set; }
        public DateTime? CreatedAt { get; set; }

        // 關聯資料
        public string? GuestName { get; set; }
        public string? Coupon { get; set; }
        public string? Room { get; set; }
        public string? HostName { get; set; }

		// 同行旅客清單
		public List<BookingGuestDto>? Guests { get; set; } = new List<BookingGuestDto>();
    }
}
