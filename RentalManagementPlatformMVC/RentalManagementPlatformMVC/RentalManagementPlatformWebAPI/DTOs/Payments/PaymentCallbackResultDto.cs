namespace RentalManagementPlatformWebAPI.DTOs.Payments
{
	public class PaymentCallbackResultDto
	{
		// 是否處理成功
		public bool Success { get; set; }

		// 訊息
		public string Message { get; set; }

		// 訂單編號
		public string OrderNumber { get; set; }

		// 更新後的付款狀態
		public string PaymentStatus { get; set; }

		// 更新後的訂單狀態
		public string OrderStatus { get; set; }
	}
}
