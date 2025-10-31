namespace RentalManagementPlatformWebAPI.DTOS
{
    public class CouponValidationRequestDto // 驗證優惠券請求的 DTO (API 使用)
    {
        public string DiscountCode { get; set; } = string.Empty; // 優惠代碼
        public decimal TotalAmount { get; set; } = 0; // 訂單總金額
        public int LeaseDays { get; set; } = 0; // 租期天數

        // 以下為可選的驗證脈絡
        public int? UserId { get; set; } // 用於驗證新用戶或生日等
        public int? CityId { get; set; } // 用於驗證地區限制
        public DateTime? UseDate { get; set; } // 預計使用日期，用於驗證租期區間
    }
}
