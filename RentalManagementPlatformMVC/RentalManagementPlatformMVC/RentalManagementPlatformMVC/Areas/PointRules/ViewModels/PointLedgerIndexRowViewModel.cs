using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
	public class PointLedgerIndexRowViewModel
	{
		[Display(Name = "流水號")]
		public int LedgerId { get; set; }

		[Display(Name = "房客編號")]
		public int? GuestId { get; set; }

		[Display(Name = "訂單編號")]
		public int? BookingId { get; set; }

		[Display(Name = "訂單號碼")]
		public string? OrderNumberSnapshot { get; set; }

		[Display(Name = "點數")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
		public int? Points { get; set; }

		[Display(Name = "發生時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? OccurredAt { get; set; }

		[Display(Name = "到期時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? ExpiresAt { get; set; }

		[Display(Name = "備註")]
		public string? Note { get; set; }

		// 導覽屬性
		[Display(Name = "房客姓名")]
		public string? GuestName { get; set; }

		[Display(Name = "點數類型")]
		public string TypeDisplay => Type switch
		{
			"earn" => "獲得",
			"redeem" => "兌換",
			_ => Type ?? "未知"
		};

		public string? Type { get; set; }
	}
}