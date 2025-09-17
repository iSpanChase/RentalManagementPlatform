namespace RentalManagementPlatformMVC.Areas.Management.DTOS
{
	public class CouponValidationResultDto
	{
		public bool IsValid { get; set; }//優惠券是否有效
		public string Message { get; set; } = string.Empty;//優惠券訊息
		
		public static CouponValidationResultDto Success()=> new CouponValidationResultDto { IsValid = true, Message = "成功使用優惠碼" };//建立靜態方法Success,將IsValid設為true,Message設為"優惠碼有效"
		public static CouponValidationResultDto Failure(string message) => new CouponValidationResultDto { IsValid = false, Message = message };//建立靜態方法Failure,將IsValid設為false,Message設為傳入的message參數(提供失敗訊息提示)
	}
}
