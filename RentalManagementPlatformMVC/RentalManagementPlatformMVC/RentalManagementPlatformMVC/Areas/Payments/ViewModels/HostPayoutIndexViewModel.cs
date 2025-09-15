using RentalManagementPlatformMVC.DTOs.Payments;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class HostPayoutIndexViewModel
	{
		public List<HostPayoutIndexRowViewModel> HostPayouts { get; set; } = new List<HostPayoutIndexRowViewModel>();
		public int PageIndex { get; set; } 
		public int PageSize { get; set; } 
		public int TotalCount { get; set; }
		public int TotalPages { get; set; } 
		public HostSearchCriteriaDto? Criteria { get; set; }
	}
}
