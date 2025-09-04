using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class HostSubscription
{
    public int HostSubId { get; set; }

    public int? HostId { get; set; }

    public int? PlanId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? NextBillingDate { get; set; }

    public bool? CancelAtPeriodEnd { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }
}
