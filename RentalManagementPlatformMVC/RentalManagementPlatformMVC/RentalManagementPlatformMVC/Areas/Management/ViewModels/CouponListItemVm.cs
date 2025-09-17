//ViewModel是作為View與Controller之間的溝通橋樑
//主要目的是為了view的呈現需求
//可以加入格式化屬性與顯示邏輯
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Management.ViewModels 
{
	public class CouponListItemVm//提供View要呈現的需求,取得所要顯示的欄位
	{
		public int CouponId { get; set; }
		public string? CouponName { get; set; }
		public string? DiscountCode { get; set; }
		public string? Description { get; set; }
		public int? MinRentalPeriod { get; set; }
		public int? MaxRentalPeriod { get; set; }
		public DateTime? StartRentalPeriod { get; set; }
		public DateTime? EndRentalPeriod { get; set; }
		public string? DiscountMethod { get; set; }
		public decimal? DiscountQuota { get; set; }
		public decimal? LowSpend { get; set; }
		public DateTime? EndAt { get; set; }
		public bool IsDeleted { get; set; }

		// 提供View格式化屬性
		public string DiscountQuotaDisplay => DiscountQuota?.ToString("N0") ?? "-"; //"N0" 是標準數字格式字串："N" → Number（數字格式,會加千分位） "0" → 小數位數（ 0 位小數,整數）
		public string LowSpendDisplay => LowSpend?.ToString("N0") ?? "-";//?.判斷是否為 NULL ; ??  "-" =>假如是 NULL 的畫會顯示 "-"
		public string EndAtDisplay => EndAt?.ToString("yyyy-MM-dd") ?? "無期限";
		public string StartRentalPeriodDisplay => StartRentalPeriod?.ToString("yyyy-MM-dd") ?? "-";
		public string EndRentalPeriodDisplay => EndRentalPeriod?.ToString("yyyy-MM-dd") ?? "-";
	}
}