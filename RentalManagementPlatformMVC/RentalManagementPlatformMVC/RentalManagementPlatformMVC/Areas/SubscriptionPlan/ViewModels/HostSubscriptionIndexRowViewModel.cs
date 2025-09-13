namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class HostSubscriptionIndexRowViewModel
	{
		public int? HostId { get; set; }
		public string? HostName { get; set; }
		public string? PlanName { get; set; } 
		public DateTime? StartDate { get; set; }
		public DateTime? NextBillingDate { get; set; }
		public bool? CancelAtPeriodEnd { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }

		// 狀態顯示為中文
		public string? DisplayStatus => Status?.ToLower() switch
		{
			"active" => "啟用中",
			"canceled" => "已取消",
			_ => "未知"
		};
	}
}
