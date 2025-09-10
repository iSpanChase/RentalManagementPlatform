using RentalManagementPlatformMVC.DTOs;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class HostPayoutDetailViewModel
	{
		// 撥款基本資料
		public int PayoutId { get; set; }
		public int? HostId { get; set; }
		[Display(Name = "房東名稱")]
		public string? HostName { get; set; }

		[Display(Name = "週期開始")]
		public DateTime? CycleStart { get; set; }

		[Display(Name = "週期結束")]
		public DateTime? CycleEnd { get; set; }

		[Display(Name = "總金額")]
		public decimal? AmountGross { get; set; }

		[Display(Name = "平台手續費")]
		public decimal? PlatformFee { get; set; }

		[Display(Name = "實際金額")]
		public decimal? AmountNet { get; set; }

		[Display(Name = "付款時間")]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		public DateTime? CreatedAt { get; set; }

		// 關聯的撥款項目
		public List<HostPayoutItemDto> Items { get; set; } = new();

		// 狀態顯示轉換
		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已出款",
			"pending" => "待出款",
			"refunded" => "已退款",
			"failed" => "失敗",
			_ => "未知"
		};

		// 撥款項目筆數
		public int ItemsCount => Items?.Count ?? 0;
	}
}
