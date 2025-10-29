namespace RentalManagementPlatformWebAPI.DTOs.Bookings
{
	/// <summary>
	/// 建立訂單並付款的回應 DTO
	/// </summary>
	public class CreateOrderAndPayResponseDto
	{
		// 訂單 ID
		public int BookingId { get; set; }

		// 訂單編號（例如：ORD202410270001）
		public string OrderNumber { get; set; }

		/// <summary>
		/// 是否需要立即付款
		/// - true = 立即支付（需要跳轉綠界）
		/// - false = 延後支付（不需要跳轉）
		/// </summary>
		public bool PaymentRequired { get; set; }

		/// <summary>
		/// 付款狀態
		/// - "pending" = 等待付款中（立即支付）
		/// - "deferred" = 延後支付
		/// - "completed" = 已完成付款
		/// - "failed" = 付款失敗
		/// </summary>
		public string PaymentStatus { get; set; }

		///綠界付款表單 HTML（只有立即支付時才有內容）
		public string? EcpayFormHtml { get; set; }

		// 付款期限（只有延後支付時才有內容）
		public DateTime? PaymentDeadline { get; set; }
	}
}
