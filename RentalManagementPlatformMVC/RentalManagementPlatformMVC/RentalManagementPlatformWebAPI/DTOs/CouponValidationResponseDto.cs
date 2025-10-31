namespace RentalManagementPlatformWebAPI.DTOS
{
    public class CouponValidationResponseDto
    {
        public bool IsValid { get; set; }
        public string Message { get; set; } = string.Empty;
        public string ErrorCode { get; set; } = string.Empty;
        public decimal? DiscountAmount { get; set; }
        public decimal? FinalPrice { get; set; }
    }
}