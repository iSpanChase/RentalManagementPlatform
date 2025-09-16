using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Payments.ViewModels
{
	public class HostPayoutIndexRowViewModel
	{
		[Display(Name = "撥款編號")]
		public int PayoutId { get; set; }

		[Display(Name = "房東ID")]
		public int? HostId { get; set; }

		[Display(Name = "房東姓名")]
		public string? HostName { get; set; }

		[Display(Name = "週期起始")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime? CycleStart { get; set; }

		[Display(Name = "週期結束")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
		public DateTime? CycleEnd { get; set; }

		[Display(Name = "總額")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public decimal? AmountGross { get; set; }

		[Display(Name = "平台費")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public decimal? PlatformFee { get; set; }

		[Display(Name = "實付")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = true)]
		public decimal? AmountNet { get; set; }

		[Display(Name = "匯款時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		public DateTime? PaidAt { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; } = "";

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = true)]
		public DateTime? CreatedAt { get; set; }

		[Display(Name = "狀態")]
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
