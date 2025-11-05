using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformWebAPI.DTOs.Bookings
{
	// 建立訂單並付款的 DTO（整合前端所有資料）
	public class CreateBookingWithPaymentDto
	{
		// ==================== 訂房基本資料 ====================
		[Required(ErrorMessage = "必須提供房客ID")]
		public int? GuestId { get; set; }

		[Required(ErrorMessage = "必須提供房間ID")]
		public int RoomId { get; set; }

		public int? CouponId { get; set; }

		[Required(ErrorMessage = "必須提供入住日期")]
		public DateTime CheckIn { get; set; }

		[Required(ErrorMessage = "必須提供退房日期")]
		public DateTime CheckOut { get; set; }

		[Required(ErrorMessage = "住宿人數為必填")]
		[Range(1, 20, ErrorMessage = "住宿人數必須介於 1-20 人")]
		public int GuestCount { get; set; }

		[Range(0, int.MaxValue, ErrorMessage = "折抵點數不能為負數")]
		public int? PointsRedeemed { get; set; }

		// ==================== 價格資訊（前端計算好的） ====================

		//[Required]
		//public int Nights { get; set; }

		//[Required]
		//public decimal PricePerNight { get; set; }

		//[Required]
		//public decimal Subtotal { get; set; }

		//public decimal DiscountAmount { get; set; }

		[Required]
		public decimal TotalPrice { get; set; }

		// ==================== Step 1: 付款時機 ====================

		[Required(ErrorMessage = "付款時機為必填")]
		public string PaymentTiming { get; set; } = "full";  // "full" 或 "partial"

		// ==================== Step 2: 聯絡資訊 ====================

		[Required(ErrorMessage = "聯絡人資訊為必填")]
		public BillingInfoDto BillingInfo { get; set; } = new();

		// ==================== Step 2: 帳單地址 ====================

		[Required(ErrorMessage = "帳單地址為必填")]
		public BillingAddressDto BillingAddress { get; set; } = new();
	}

	// 聯絡人資訊
	public class BillingInfoDto
	{
		[Required(ErrorMessage = "姓名為必填")]
		[StringLength(100, ErrorMessage = "姓名長度不可超過 100 字")]
		public string Name { get; set; } = string.Empty;

		[Required(ErrorMessage = "Email 為必填")]
		[EmailAddress(ErrorMessage = "Email 格式不正確")]
		[StringLength(100)]
		public string Email { get; set; } = string.Empty;

		[Required(ErrorMessage = "電話為必填")]
		[Phone(ErrorMessage = "電話格式不正確")]
		[StringLength(50)]
		public string Phone { get; set; } = string.Empty;

		[StringLength(500)]
		public string? Notes { get; set; }
	}

	// 帳單地址
	public class BillingAddressDto
	{
		[Required(ErrorMessage = "國家為必填")]
		[StringLength(10)]
		public string Country { get; set; } = string.Empty;

		[Required(ErrorMessage = "街道地址為必填")]
		[StringLength(200)]
		public string Street { get; set; } = string.Empty;

		[StringLength(100)]
		public string? Apartment { get; set; }

		[Required(ErrorMessage = "城市為必填")]
		[StringLength(100)]
		public string City { get; set; } = string.Empty;

		[StringLength(100)]
		public string? State { get; set; }

		[Required(ErrorMessage = "郵遞區號為必填")]
		[StringLength(20)]
		public string ZipCode { get; set; } = string.Empty;
	}
}
