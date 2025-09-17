using RentalManagementPlatformMVC.DTOs.Payments;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class HostPayoutDetailViewModel
	{
		// 撥款基本資料
		[Display(Name = "撥款編號")]
		public int PayoutId { get; set; }

		[Display(Name = "房東ID")]
		public int? HostId { get; set; }

		[Display(Name = "房東名稱")]
		public string? HostName { get; set; }

		[Display(Name = "週期開始")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? CycleStart { get; set; }

		[Display(Name = "週期結束")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? CycleEnd { get; set; }

		[Display(Name = "總金額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? AmountGross { get; set; }

		[Display(Name = "平台手續費")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? PlatformFee { get; set; }

		[Display(Name = "實際金額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? AmountNet { get; set; }

		[Display(Name = "付款時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? CreatedAt { get; set; }

		// 關聯的撥款項目
		[Display(Name = "撥款項目")]
		public List<HostPayoutItemDto> Items { get; set; } = new();

		// 狀態顯示轉換
		[Display(Name = "狀態")]
		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已出款",
			"pending" => "待出款",
			"refunded" => "已退款",
			"failed" => "失敗",
			_ => "未知"
		};

		// 撥款項目筆數
		[Display(Name = "項目數量")]
		public int ItemsCount => Items?.Count ?? 0;
	}
}
