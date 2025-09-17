namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlans
{
	public class HostSubscriptionDetailDto
	{
		// 基本訂閱資料
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

		// 帳單紀錄 (一對多關聯)
		public List<HostSubscirptionBillingDto> Billings { get; set; } = new();
	}
}
