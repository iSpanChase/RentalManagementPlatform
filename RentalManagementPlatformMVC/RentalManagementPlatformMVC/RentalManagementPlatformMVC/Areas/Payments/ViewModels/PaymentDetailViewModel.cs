using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentDetailViewModel
	{
		// === Payment 基本資訊 ===
		public int PaymentId { get; set; }
		public int? BookingId { get; set; }
		public decimal? Amount { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? PaymentRef { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public string? Status { get; set; }
		public DateTime? PaymentCreatedAt { get; set; }

		// === Transaction 詳細資訊 ===
		public int TransactionId { get; set; }
		public string? ProviderTxnId { get; set; }
		public string? ResponseCode { get; set; }
		public string? Provider { get; set; }
		public string? ResponseMessage { get; set; }
		public string? TxnRef { get; set; }
		public DateTime? TransactionCreatedAt { get; set; }

		// === Booking 額外顯示用 ===
		public string? GuestName { get; set; }
		public string? RoomTitle { get; set; }

		// === 顯示用屬性 ===
		public string DisplayMethod => Method?.ToLower() switch
		{
			"credit_card" => "信用卡",
			_ => "其他"
		};
		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已付款",
			"pending" => "付款中",
			"refunded" => "已退款",
			"failed" => "付款失敗",
			_ => "未知"
		};

		[Display(Name = "回應訊息")]
		public string DisplayResponseMessage =>
		ResponseMessage?.ToLower() switch
		{
		   "approved" => "付款成功",
		   "refunded" => "已退款",
		   "do not honor" => "拒絕交易",
		   "processing" => "處理中",
		   _ => "未知"
		};
	}
}
