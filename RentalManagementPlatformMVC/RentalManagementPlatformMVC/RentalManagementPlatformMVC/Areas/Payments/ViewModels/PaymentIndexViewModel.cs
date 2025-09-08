using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentIndexViewModel
	{
		public List<PaymentIndexRowViewModel> Payments { get; set; } = new();
		public int PageIndex { get; set; }
		public int PageSize { get; set; }
		public int TotalPages { get; set; }
		public int TotalCount { get; set; }
		public PaymentSearchCriteriaDto? Criteria { get; set; }
	}
}
