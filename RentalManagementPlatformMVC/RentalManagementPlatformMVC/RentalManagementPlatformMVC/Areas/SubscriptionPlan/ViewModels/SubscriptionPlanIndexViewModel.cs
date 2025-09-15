namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class SubscriptionPlanIndexViewModel
	{
		public List<SubscriptionPlanIndexRowViewModel> Plans { get; set; } = new();
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
	}
}
