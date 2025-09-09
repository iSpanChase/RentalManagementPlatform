namespace RentalManagementPlatformMVC.DTOs
{
	public class PaymentDetailDto
	{
		// === Payment 基本資訊 ===
		public int PaymentId { get; set; }
		public int? BookingId { get; set; }
		public decimal? Amount { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? PaymentRef { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public string? Status { get; set; }
		public DateTime? PaymentCreatedAt { get; set; }

		// === Booking 資訊 ===
		public string? GuestName { get; set; }
		public string? RoomTitle { get; set; }

		// === 多筆 Transaction ===
		public List<PaymentTransactionDto> Transactions { get; set; } = new();
	}
}