//ViewModel是偏向前端UI的表現,且包含了(驗證規則)和顯示格式化(屬性)。

namespace RentalManagementPlatformMVC.Areas.Management.ViewModels 
{
	public class CouponListItemVm
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

		// 格式化屬性,供前端顯示使用
		public string DiscountQuotaDisplay => DiscountQuota?.ToString("N0") ?? "-"; //"N0" 是 標準數字格式字串："N" → Number（數字格式，會自動加上千分位符號） "0" → 小數位數（這裡是 0 位小數，也就是整數）
		public string LowSpendDisplay => LowSpend?.ToString("N0") ?? "-";
		public string EndAtDisplay => EndAt?.ToString("yyyy-MM-dd") ?? "無期限";
		public string StartRentalPeriodDisplay => StartRentalPeriod?.ToString("yyyy-MM-dd") ?? "-";
		public string EndRentalPeriodDisplay => EndRentalPeriod?.ToString("yyyy-MM-dd") ?? "-";
	}
}