using Humanizer;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class PaymentIndexRowViewModel
	{
		[Display(Name = "付款編號")]
		public int PaymentId { get; set; }

		[Display(Name = "訂單編號")]
		[StringLength(50, ErrorMessage = "訂單編號長度不可超過 50 字元")]
		public string? OrderNumberSnapshot { get; set; }

		[Display(Name = "付款金額")]
		[DataType(DataType.Currency)]
		[Range(0, double.MaxValue, ErrorMessage = "金額必須為正數")]
		public decimal? Amount { get; set; }

		[Display(Name = "付款參考號")]
		[StringLength(100, ErrorMessage = "參考號長度不可超過 100 字元")]
		public string? PaymentRef { get; set; }

		[Display(Name = "付款方式")]
		[StringLength(30)]
		public string? Method { get; set; }

		[Display(Name = "付款時間")]
		[DataType(DataType.DateTime)]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "付款狀態")]
		[StringLength(30)]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DataType(DataType.DateTime)]
		public DateTime? CreatedAt { get; set; }

		// === 顯示用屬性 ===
		[Display(Name = "付款方式")]
		public string DisplayMethod => Method?.ToLower() switch
		{
			"credit_card" => "信用卡",
			_ => "其他"
		};

		[Display(Name = "付款狀態")]
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