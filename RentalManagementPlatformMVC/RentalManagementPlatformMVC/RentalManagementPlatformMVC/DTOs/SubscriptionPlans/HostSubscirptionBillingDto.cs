namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlans
{
	public class HostSubscirptionBillingDto
	{
		public int BillId { get; set; }

		public int? HostSubId { get; set; }

		public decimal? Amount { get; set; }

		public string? PaidStatus { get; set; }

		public DateTime? PaidAt { get; set; }

		public DateTime? CreatedAt { get; set; }

		public string? Note { get; set; } = string.Empty;
	}
}
