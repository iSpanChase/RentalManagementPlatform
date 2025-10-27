namespace RentalManagementPlatformWebAPI.DTOs.Bookings
{
	/// <summary>
	/// 建立訂單並付款的回應 DTO
	/// </summary>
	public class CreateOrderAndPayResponseDto
	{
		/// <summary>
		/// 訂單 ID
		/// </summary>
		public int BookingId { get; set; }

		/// <summary>
		/// 訂單編號（例如：ORD202412250001）
		/// </summary>
		public string OrderNumber { get; set; } = string.Empty;

		/// <summary>
		/// 綠界付款表單 HTML（包含自動提交的 JavaScript）
		/// </summary>
		public string EcpayFormHtml { get; set; } = string.Empty;
	}
}
