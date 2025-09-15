using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
	public class PointRuleIndexRowViewModel
	{
		public int RuleId { get; set; }

		[Display(Name = "每台幣獲得點數")]
		public decimal? EarnRatePerNtd { get; set; }

		[Display(Name = "每訂單最高點數")]
		public int? MaxPointsPerOrder { get; set; }

		[Display(Name = "點數有效月數")]
		public int? ExpiryMonths { get; set; }

		[Display(Name = "每點兌換台幣")]
		public decimal? RedeemRateNtdPerPt { get; set; }

		[Display(Name = "生效日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? ActiveFrom { get; set; }

		[Display(Name = "失效日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? ActiveTo { get; set; }

		[Display(Name = "狀態")]
		public string StatusDisplay => IsActive ? "已啟用" : "未啟用";
		public bool IsActive { get; set; }

		[Display(Name = "建立時間")]
		public DateTime? CreatedAt { get; set; }

		// 操作權限（前端按鈕顯示用）
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool CanActivate { get; set; }
		public bool CanDeactivate { get; set; }
	}
}
