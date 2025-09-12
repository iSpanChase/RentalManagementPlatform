//CouponGuest中含有coupon和user的資料
namespace RentalManagementPlatformMVC.Areas.Management.DTOS
{
	public class CouponGuestDto
	{
		public int CouponGuestId { get; set; }
		public int? CouponId { get; set; }
		public int? GuestId { get; set; }

		public string? CouponName { get; set; }//coupon名稱(coupon中提取)
		public string? DiscountCode { get; set; }//coupon優惠代碼(coupon中提取)
		public string Username { get; set; } = null!;//user的名字(user中提取)

		public DateTime? CreateAt { get; set; }
		public DateTime? RemoveAt { get; set; }
	}
}
