using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class HostSubscriptionDetailViewModel
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

		// 帳單紀錄
		public List<HostSubscriptionBillingViewModel> Billings { get; set; } = new();

		// === 顯示用屬性 ===
		public string DisplayStatus =>
			Status?.ToLower() switch
			{
				"active" => "啟用中",
				"canceled" => "已取消",
				"expired" => "已到期",
				_ => "未知"
			};

		public string DisplayStartDate => StartDate?.ToString("yyyy-MM-dd") ?? "-";
		public string DisplayNextBillingDate => NextBillingDate?.ToString("yyyy-MM-dd") ?? "-";
		public string DisplayCreatedAt => CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
		public string DisplayCancelAtPeriodEnd => CancelAtPeriodEnd == true ? "是" : "否";
	}

	public class HostSubscriptionBillingViewModel
	{
		public int BillId { get; set; }
		public int? HostSubId { get; set; }
		public decimal? Amount { get; set; }
		public string? PaidStatus { get; set; }
		public DateTime? PaidAt { get; set; }
		public DateTime? CreatedAt { get; set; }
		public string? Note { get; set; }

		// === 顯示用屬性 ===
		public string DisplayPaidStatus =>
			PaidStatus?.ToLower() switch
			{
				"paid" => "已付款",
				"pending" => "待付款",
				"overdue" => "逾期",
				"failed" => "付款失敗",
				_ => "未知"
			};

		public string DisplayAmount => Amount?.ToString("N0") ?? "-";
		public string DisplayPaidAt => PaidAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
		public string DisplayCreatedAt => CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
	}
}
