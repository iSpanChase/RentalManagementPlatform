using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlan
{
	public class SubscriptionPlanDto
	{
		public int PlanId { get; set; }

		[Required]
		[StringLength(10, MinimumLength = 1)]
		public string PlanName { get; set; } = string.Empty;

		[Range(0, double.MaxValue)]
		public decimal MonthlyFee { get; set; }

		[Range(0.0001, 1.0000)]
		public decimal CommissionRate { get; set; }

		public bool PerkPriority { get; set; }
		public bool PerkAnalytics { get; set; }
		public bool IsActive { get; set; }
		public DateTime CreatedAt { get; set; }

		// 商業邏輯屬性
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool CanActivate { get; set; }
		public bool CanDeactivate { get; set; }
		public int SubscriberCount { get; set; }

		// 計算屬性 - 提供更好的使用體驗
		public string MonthlyFeeDisplay => $"${MonthlyFee:N0}";
		public string CommissionRateDisplay => $"{CommissionRate:P2}";
		public string StatusDisplay => IsActive ? "已啟用" : "未啟用";
		public string PerksSummary => GetPerksSummary();

		private string GetPerksSummary()
		{
			var perks = new List<string>();
			if (PerkPriority) perks.Add("優先曝光");
			if (PerkAnalytics) perks.Add("數據分析");
			return perks.Any() ? string.Join(", ", perks) : "無特殊權益";
		}
	}
}
