using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.PointRules
{
	public class EditPointRuleDto
	{
		[Required]
		public int RuleId { get; set; }

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal EarnRatePerNtd { get; set; }

		[Range(0, int.MaxValue)]
		public int? MaxPointsPerOrder { get; set; }

		[Range(1, 120)]
		public int? ExpiryMonths { get; set; }

		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal RedeemRateNtdPerPt { get; set; }

		[DataType(DataType.DateTime)]
		public DateTime? ActiveFrom { get; set; }

		[DataType(DataType.DateTime)]
		public DateTime? ActiveTo { get; set; }
	}
}