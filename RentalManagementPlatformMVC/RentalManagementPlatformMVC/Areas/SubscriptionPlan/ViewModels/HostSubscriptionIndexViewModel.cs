using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels
{
	public class HostSubscriptionIndexViewModel
	{
		public List<HostSubscriptionDto> HostSubscriptions { get; set; } = new();
		public HostSubscriptionSearchCriteriaDto SearchCriteria { get; set; } = new();
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
	}
}