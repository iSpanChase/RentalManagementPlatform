using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.SubscriptionPlan
{
	public class CreatePlanDto
	{
		[Required]
		[StringLength(10, MinimumLength = 1)]
		public string PlanName { get; set; } = string.Empty;

		[Range(0, double.MaxValue)]
		public decimal MonthlyFee { get; set; }

		[Range(0.0001, 1.0000)]
		public decimal CommissionRate { get; set; }

		public bool PerkPriority { get; set; }

		public bool PerkAnalytics { get; set; }
	}
}
