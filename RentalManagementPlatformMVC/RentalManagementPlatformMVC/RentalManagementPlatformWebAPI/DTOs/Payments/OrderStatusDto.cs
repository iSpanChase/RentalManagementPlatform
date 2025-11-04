namespace RentalManagementPlatformWebAPI.DTOs.Payments
{
	public class OrderStatusDto
	{
		/// <summary>
		/// 訂單編號
		/// </summary>
		public string OrderNumber { get; set; }

		/// <summary>
		/// 付款狀態
		/// </summary>
		public string PaymentStatus { get; set; }

		/// <summary>
		/// 訂單狀態
		/// </summary>
		public string OrderStatus { get; set; }

		/// <summary>
		/// 最後更新時間
		/// </summary>
		public DateTime LastUpdated { get; set; }
	}
}