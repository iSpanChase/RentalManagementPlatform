namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
	/// <summary>
	/// 訂單管理清單頁中，用來呈現單筆訂單的 ViewModel。
	/// </summary>
	public class BookingIndexRowViewModel
	{
		public int BookingId { get; set; }
		public string? OrderNumber { get; set; }
		public string? GuestName { get; set; }
		public DateTime? CheckIn { get; set; }
		public DateTime? CheckOut { get; set; }
		public decimal? TotalPrice { get; set; }
		public string? Status { get; set; }
		public DateTime? CreatedAt { get; set; }

		// 狀態顯示為中文
		public string DisplayStatus => Status?.ToLower() switch
		{
			"confirmed" => "已確認",
			"pending" => "待確認",
			"cancelled" => "已取消",
			_ => "未知"
		};
	}
}
