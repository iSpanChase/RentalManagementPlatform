using RentalManagementPlatformMVC.DTOs;

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


		// （可選）本頁合計，放在工具列顯示
		public decimal PageTotalGross { get; set; }  // 本頁總額（未扣）
		public decimal PageTotalFee { get; set; }  // 本頁平台費合計
		public decimal PageTotalNet { get; set; }  // 本頁實際出款合計
	}
}
