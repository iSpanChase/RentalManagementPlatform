namespace RentalManagementPlatformWebAPI.DTOS
{
	public class RedeemPromoCodeRequestDto//兌換優惠碼請求的DTO
	{
		public string DiscountCode { get; set; } = string.Empty;//優惠代碼
		public int UserId { get; set; }//使用者ID
	}
}
