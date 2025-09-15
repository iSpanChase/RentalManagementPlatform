namespace RentalManagementPlatformMVC.DTOs.PointRules
{
	public class PointRuleDto
	{
		public int RuleId { get; set; }

		public decimal? EarnRatePerNtd { get; set; }

		public int? MaxPointsPerOrder { get; set; }

		public int? ExpiryMonths { get; set; }

		public decimal? RedeemRateNtdPerPt { get; set; }

		public DateTime? ActiveFrom { get; set; }

		public DateTime? ActiveTo { get; set; }

		public bool? IsActive { get; set; }

		public DateTime? CreatedAt { get; set; }

		// 商業邏輯屬性
		public bool CanEdit { get; set; }
		public bool CanDelete { get; set; }
		public bool CanActivate { get; set; }
		public bool CanDeactivate { get; set; }
	}
}
