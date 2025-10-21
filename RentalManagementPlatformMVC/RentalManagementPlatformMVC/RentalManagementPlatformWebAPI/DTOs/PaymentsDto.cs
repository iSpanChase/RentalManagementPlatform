namespace RentalManagementPlatformWebAPI.DTOs
{
	public class PaymentsDto
	{
		public int PaymentId { get; set; }
		public int? BookingId { get; set; }
		public decimal? Amount { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? PaymentRef { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
