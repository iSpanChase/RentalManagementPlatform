using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.PointRules
{
    public class CreatePointRuleDto
    {
		[Required]
		[Range(0.01, double.MaxValue)]
		public decimal EarnRatePerNtd { get; set; }

		[Required]
		[Range(0, int.MaxValue)]
		public int? MaxPointsPerOrder { get; set; }

		[Required]
		[Range(1, 120)]
		public int? ExpiryMonths { get; set; }

		[Required]
		[Range(1, double.MaxValue)]
		public decimal RedeemRateNtdPerPt { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime? ActiveFrom { get; set; }

		[Required]
		[DataType(DataType.DateTime)]
		public DateTime? ActiveTo { get; set; }
	}
}
