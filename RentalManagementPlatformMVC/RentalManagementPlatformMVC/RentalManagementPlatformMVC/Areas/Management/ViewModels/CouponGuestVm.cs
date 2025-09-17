namespace RentalManagementPlatformMVC.Areas.Management.ViewModels
{
	public class CouponGuestVm
	//提供view要顯示的資料
	{
		public int CouponGuestId { get; set; }
		public int? CouponId { get; set; }
		public int? GuestId { get; set; }

		public string? CouponName { get; set; }//coupon名稱(coupon中提取)
		public string? DiscountCode { get; set; }//coupon優惠代碼(coupon中提取)
		public string Name { get; set; } = null!;//user的名字(user中提取)

		public DateTime? CreateAt { get; set; }
		public DateTime? RemoveAt { get; set; }

		//提供view格式化屬性
		public string CreateAtDisplay => CreateAt?.ToString("yyyy-MM-dd") ?? "-";
		public string RemoveAtDisplay => RemoveAt?.ToString("yyyy-MM-dd") ?? "-";
	}
}
