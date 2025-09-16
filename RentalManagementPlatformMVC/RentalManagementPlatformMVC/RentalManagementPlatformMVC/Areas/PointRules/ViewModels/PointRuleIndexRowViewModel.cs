using DocumentFormat.OpenXml.Wordprocessing;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.PointRules.ViewModels
{
	public class PointRuleIndexRowViewModel
	{
		[Display(Name = "規則編號")]
		public int RuleId { get; set; }

		[Display(Name = "每台幣獲得點數")]
		[DisplayFormat(DataFormatString = "{0:N2}", ApplyFormatInEditMode = false)]
		public decimal? EarnRatePerNtd { get; set; }

		[Display(Name = "每訂單最高點數")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
		public int? MaxPointsPerOrder { get; set; }

		[Display(Name = "點數有效月數")]
		[DisplayFormat(DataFormatString = "{0:N0}", ApplyFormatInEditMode = false)]
		public int? ExpiryMonths { get; set; }

		[Display(Name = "每點兌換台幣")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
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
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? CreatedAt { get; set; }

		// 操作權限
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool CanActivate { get; set; }
		public bool CanDeactivate { get; set; }
	}
}
