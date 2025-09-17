using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class EditPlanViewModel
	{
		[Required]
		public int PlanId { get; set; }

		[Required(ErrorMessage = "方案名稱不能為空")]
		[StringLength(10, MinimumLength = 1, ErrorMessage = "方案名稱長度必須在1-10字元之間")]
		[Display(Name = "方案名稱")]
		public string PlanName { get; set; } = string.Empty;

		[Range(0, double.MaxValue, ErrorMessage = "月費不能為負數")]
		[Display(Name = "月費")]
		public decimal MonthlyFee { get; set; }

		[Range(0.0001, 1.0000, ErrorMessage = "佣金率必須在0.0001-1.0000之間")]
		[Display(Name = "佣金率")]
		public decimal CommissionRate { get; set; }

		[Display(Name = "優先權限")]
		public bool PerkPriority { get; set; }

		[Display(Name = "分析功能")]
		public bool PerkAnalytics { get; set; }
	}
}