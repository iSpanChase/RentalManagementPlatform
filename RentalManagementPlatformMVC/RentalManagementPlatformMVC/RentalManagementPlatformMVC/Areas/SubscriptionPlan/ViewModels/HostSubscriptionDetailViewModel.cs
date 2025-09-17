using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class HostSubscriptionDetailViewModel
	{
		// 基本訂閱資料
		[Display(Name = "訂閱編號")]
		public int HostSubId { get; set; }

		[Display(Name = "房東編號")]
		public int? HostId { get; set; }

		[Display(Name = "方案編號")]
		public int? PlanId { get; set; }

		[Display(Name = "開始日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? StartDate { get; set; }

		[Display(Name = "下次計費日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? NextBillingDate { get; set; }

		[Display(Name = "週期結束時取消")]
		public bool? CancelAtPeriodEnd { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? CreatedAt { get; set; }

		// 關聯資料
		[Display(Name = "房東姓名")]
		public string? HostName { get; set; }

		[Display(Name = "方案名稱")]
		public string? PlanName { get; set; }

		// 帳單紀錄
		[Display(Name = "訂閱帳單")]
		public List<HostSubscriptionBillingViewModel> Billings { get; set; } = new();

		// === 顯示用屬性 ===
		[Display(Name = "狀態")]
		public string DisplayStatus =>
			Status?.ToLower() switch
			{
				"active" => "啟用中",
				"canceled" => "已取消",
				"expired" => "已到期",
				_ => "未知"
			};

		[Display(Name = "開始日期")]
		public string DisplayStartDate => StartDate?.ToString("yyyy-MM-dd") ?? "-";

		[Display(Name = "下次計費日期")]
		public string DisplayNextBillingDate => NextBillingDate?.ToString("yyyy-MM-dd") ?? "-";

		[Display(Name = "建立時間")]
		public string DisplayCreatedAt => CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

		[Display(Name = "週期結束時取消")]
		public string DisplayCancelAtPeriodEnd => CancelAtPeriodEnd == true ? "是" : "否";
	}

	public class HostSubscriptionBillingViewModel
	{
		[Display(Name = "帳單編號")]
		public int BillId { get; set; }

		[Display(Name = "訂閱編號")]
		public int? HostSubId { get; set; }

		[Display(Name = "金額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? Amount { get; set; }

		[Display(Name = "付款狀態")]
		public string? PaidStatus { get; set; }

		[Display(Name = "付款時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? CreatedAt { get; set; }

		[Display(Name = "備註")]
		public string? Note { get; set; }

		// === 顯示用屬性 ===
		[Display(Name = "付款狀態")]
		public string DisplayPaidStatus =>
			PaidStatus?.ToLower() switch
			{
				"paid" => "已付款",
				"pending" => "待付款",
				"overdue" => "逾期",
				"failed" => "付款失敗",
				_ => "未知"
			};

		[Display(Name = "金額")]
		public string DisplayAmount => Amount?.ToString("N0") ?? "-";

		[Display(Name = "付款時間")]
		public string DisplayPaidAt => PaidAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";

		[Display(Name = "建立時間")]
		public string DisplayCreatedAt => CreatedAt?.ToString("yyyy-MM-dd HH:mm") ?? "-";
	}
}
