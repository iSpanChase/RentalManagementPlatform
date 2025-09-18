//dto 資料傳輸物件
//servics層與controller層之間的資料結構
namespace RentalManagementPlatformMVC.Areas.Management.DTOS
{
	public class CouponGuestDto
	{
		public int CouponGuestId { get; set; }
		public int? CouponId { get; set; }
		public int? GuestId { get; set; }

		public string? CouponName { get; set; }//coupon名稱(coupon中提取)
		public string? DiscountCode { get; set; }//coupon優惠代碼(coupon中提取)
		public string Name { get; set; } = null!;//user的名字(user中提取)

		public DateTime? CreateAt { get; set; }
		public DateTime? RemoveAt { get; set; }
	}
}
