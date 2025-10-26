namespace RentalManagementPlatformWebAPI.DTOs
{
	public class BookingDto
	{
		public int BookingId { get; set; }
		public int? CouponId { get; set; }
		public int? GuestId { get; set; }
		public int? RoomId { get; set; }
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
		public string? Room { get; set; }
	}
}
