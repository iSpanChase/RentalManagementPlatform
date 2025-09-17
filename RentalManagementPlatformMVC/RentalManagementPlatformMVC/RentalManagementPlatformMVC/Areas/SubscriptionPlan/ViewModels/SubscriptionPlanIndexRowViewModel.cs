using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class SubscriptionPlanIndexRowViewModel
	{
		public int PlanId { get; set; }

		[Display(Name = "方案名稱")]
		public string PlanName { get; set; } = string.Empty;

		[Display(Name = "月費")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}")]
		public decimal MonthlyFee { get; set; }

		[Display(Name = "平台抽成")]
		[DisplayFormat(DataFormatString = "{0:P2}")]
		public decimal CommissionRate { get; set; }

		public bool PerkPriority { get; set; }
		public bool PerkAnalytics { get; set; }

		[Display(Name = "用戶權益")]
		public string PerksDisplay => GetPerksDisplay();

		[Display(Name = "狀態")]
		public string StatusDisplay => IsActive ? "已啟用" : "未啟用";
		public bool IsActive { get; set; }

		[Display(Name = "建立日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy/MM/dd}")]
		public DateTime? CreatedAt { get; set; }

		// 操作權限（前端按鈕顯示用）
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool CanActivate { get; set; }
		public bool CanDeactivate { get; set; }

		private string GetPerksDisplay()
		{
			var perks = new List<string>();
			if (PerkPriority) perks.Add("優先曝光");
			if (PerkAnalytics) perks.Add("數據分析");

			return perks.Count > 0 ? string.Join(", ", perks) : "無特殊權益";
		}

		public string DisplayName => PlanName?.ToLower() switch
		{
			"normal" => "一般會員",
			"subscription" => "訂閱會員",
			_ => PlanName
		};
	}
}
