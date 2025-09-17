using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class PointRule
{
    public int RuleId { get; set; }

    public decimal? EarnRatePerNtd { get; set; }

    public int? MaxPointsPerOrder { get; set; }

    public int? ExpiryMonths { get; set; }

    public decimal? RedeemRateNtdPerPt { get; set; }

    public DateTime? ActiveFrom { get; set; }

    public DateTime? ActiveTo { get; set; }

    public bool? IsActive { get; set; }
	public bool? HasBeenActivated { get; set; }

	public DateTime? CreatedAt { get; set; }
}
