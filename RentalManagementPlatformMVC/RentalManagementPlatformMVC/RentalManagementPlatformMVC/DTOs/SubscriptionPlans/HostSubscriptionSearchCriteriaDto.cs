namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlans
{
	public class HostSubscriptionSearchCriteriaDto
	{
		// 基本條件
		public int? HostId { get; set; }
		public int? PlanId { get; set; }
		public string? Status { get; set; }

		// 日期範圍
		public DateTime? StartDateFrom { get; set; }
		public DateTime? StartDateTo { get; set; }
		public DateTime? CreatedAtFrom { get; set; }
		public DateTime? CreatedAtTo { get; set; }
		public DateTime? NextBillingDateFrom { get; set; }
		public DateTime? NextBillingDateTo { get; set; }

		// 關聯查詢
		public string? HostName { get; set; }
		public string? PlanName { get; set; }

		// 分頁與排序
		public string? SortBy { get; set; } = "CreatedAt";
		public bool IsDescending { get; set; } = true;
	}
}
