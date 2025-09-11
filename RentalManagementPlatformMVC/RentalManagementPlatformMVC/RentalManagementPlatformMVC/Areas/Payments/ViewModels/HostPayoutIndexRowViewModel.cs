using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class HostPayoutIndexRowViewModel
	{
		public int PayoutId { get; set; }
		public int? HostId { get; set; }

		[Display(Name = "房東姓名")]
		public string? HostName { get; set; }

		[Display(Name = "週期起始")]
		public DateTime? CycleStart { get; set; }

		[Display(Name = "週期結束")]
		public DateTime? CycleEnd { get; set; }

		[Display(Name = "總額")]
		public decimal? AmountGross { get; set; }

		[Display(Name = "平台費")]
		public decimal? PlatformFee { get; set; }

		[Display(Name = "實付")]
		public decimal? AmountNet { get; set; }

		[Display(Name = "匯款時間")]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; } = "";

		[Display(Name = "建立時間")]
		public DateTime? CreatedAt { get; set; }

		public string DisplayStatus => Status?.ToLower() switch
		{
			"paid" => "已匯款",
			"pending" => "待出款",
			"failed" => "出款失敗",
			"cancelled" => "已取消",
			_ => "未知"
		};
	}
}
