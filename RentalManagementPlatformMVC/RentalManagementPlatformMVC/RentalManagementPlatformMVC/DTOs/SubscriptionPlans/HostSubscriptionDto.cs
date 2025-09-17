namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlans
{
	public class HostSubscriptionDto
	{
		public int HostSubId { get; set; }

		public int? HostId { get; set; }

		public int? PlanId { get; set; }

		public DateTime? StartDate { get; set; }

		public DateTime? NextBillingDate { get; set; }

		public bool? CancelAtPeriodEnd { get; set; }

		public string? Status { get; set; }

		public DateTime? CreatedAt { get; set; }

		// 關聯資料
		public string? HostName { get; set; }
		public string? PlanName { get; set; }
	}
}
