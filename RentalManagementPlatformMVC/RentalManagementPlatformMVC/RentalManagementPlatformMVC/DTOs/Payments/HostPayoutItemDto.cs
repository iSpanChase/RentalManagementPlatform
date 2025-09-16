using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.DTOs.Payments
{
	public class HostPayoutItemDto
	{
		[Display(Name = "明細編號")]
		public int PayoutItemId { get; set; }

		[Display(Name = "撥款編號")]
		public int? PayoutId { get; set; }

		[Display(Name = "訂單編號")]
		public int? BookingId { get; set; }

		[Display(Name = "訂單號碼")]
		public string? OrderNumberSnapshot { get; set; }

		[Display(Name = "毛額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? AmountGross { get; set; }

		[Display(Name = "抽成比例")]
		[DisplayFormat(DataFormatString = "{0:P0}", ApplyFormatInEditMode = false)]
		public decimal? CommissionPct { get; set; }

		[Display(Name = "平台手續費")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? PlatformFee { get; set; }

		[Display(Name = "淨額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? Amount { get; set; }
	}
}
