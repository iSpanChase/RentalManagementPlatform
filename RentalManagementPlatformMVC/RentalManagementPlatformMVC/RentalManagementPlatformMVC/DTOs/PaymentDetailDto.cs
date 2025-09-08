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

		// === Booking (額外顯示用，可選) ===
		public string? GuestName { get; set; }
		public string? RoomTitle { get; set; }

		// === Transaction 詳細資訊 ===
		public int TransactionId { get; set; }
		public string? ProviderTxnId { get; set; }
		public string? ResponseCode { get; set; }
		public string? Provider { get; set; }
		public string? ResponseMessage { get; set; }
		public string? TxnRef { get; set; }
		public DateTime? TransactionCreatedAt { get; set; }
	}
}
