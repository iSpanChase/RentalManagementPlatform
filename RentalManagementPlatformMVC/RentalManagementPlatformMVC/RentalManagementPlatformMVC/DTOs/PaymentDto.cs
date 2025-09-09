namespace RentalManagementPlatformMVC.DTOs
{
	public class PaymentDto
	{
		public int PaymentId { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public decimal? Amount { get; set; }
		public string? PaymentRef { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
	}
}
