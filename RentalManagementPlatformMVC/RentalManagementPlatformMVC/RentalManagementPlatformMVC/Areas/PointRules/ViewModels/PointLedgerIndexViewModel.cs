using RentalManagementPlatformMVC.DTOs.PointRules;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
    public class PointLedgerIndexViewModel
    {
        public List<PointLedgerIndexRowViewModel> PointLedgers { get; set; } = new();

        public int PageIndex { get; set; }

        public int PageSize { get; set; }

        public int TotalPages { get; set; }

        public int TotalCount { get; set; }

        public PointLedgerSearchCriteriaDto? Criteria { get; set; }
    }
}