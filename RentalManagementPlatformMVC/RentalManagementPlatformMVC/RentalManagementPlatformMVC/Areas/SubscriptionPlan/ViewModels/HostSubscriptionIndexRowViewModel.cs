using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class HostSubscriptionIndexRowViewModel
	{
		[Display(Name = "訂閱編號")]
		public int HostSubId { get; set; }

		[Display(Name = "房東編號")]
		public int? HostId { get; set; }

		[Display(Name = "房東姓名")]
		public string? HostName { get; set; }

		[Display(Name = "方案名稱")]
		public string? PlanName { get; set; }

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

		// 狀態顯示為中文
		[Display(Name = "狀態")]
		public string? DisplayStatus => Status?.ToLower() switch
		{
			"active" => "啟用中",
			"canceled" => "已取消",
			_ => "未知"
		};

		[Display(Name = "方案名稱")]
		public string DisplayName => PlanName?.ToLower() switch
		{
			"normal" => "一般會員",
			"subscription" => "訂閱會員",
			_ => PlanName
		};

		[Display(Name = "週期結束時取消")]
		public string DisplayCancelAtPeriodEnd =>
		CancelAtPeriodEnd == true ? "是" :
		CancelAtPeriodEnd == false ? "否" : "未設定";
	}
}
