using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
	/// <summary>
	/// 訂單管理清單頁中，用來呈現單筆訂單的 ViewModel。
	/// </summary>
	public class BookingIndexRowViewModel
	{
		[Display(Name = "訂單ID")]
		public int BookingId { get; set; }

		[Display(Name = "訂單編號")]
		public string? OrderNumber { get; set; }

		[Display(Name = "房客姓名")]
		public string? GuestName { get; set; }

		[Display(Name = "入住日期")]
		[DataType(DataType.Date)]
		public DateTime? CheckIn { get; set; }

		[Display(Name = "退房日期")]
		[DataType(DataType.Date)]
		public DateTime? CheckOut { get; set; }

		[Display(Name = "總金額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}")]
		public decimal? TotalPrice { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}")]
		public DateTime? CreatedAt { get; set; }

		// 狀態顯示為中文
		public string DisplayStatus => Status?.ToLower() switch
		{
			"completed" => "已完成",
			"confirmed" => "已確認",
			"pending" => "待確認",
			"cancelled" => "已取消",
			_ => "未知"
		};
	}
}
