using Humanizer;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentIndexRowViewModel
	{
		public int PaymentId { get; set; }
		public string? OrderNumberSnapshot { get; set; }
		public decimal? Amount { get; set; }
		public string? PaymentRef { get; set; }
		public string? Method { get; set; }
		public DateTime? PaidAt { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }
		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已付款",
			"pending" => "付款中",
			"refunded" => "已退款",
			"failed" => "付款失敗",
			_ => "未知"
		};
	}
}
