using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
	public class PointRuleIndexViewModel
	{
		public List<PointRuleIndexRowViewModel> Rules { get; set; } = new();
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
	}
}
