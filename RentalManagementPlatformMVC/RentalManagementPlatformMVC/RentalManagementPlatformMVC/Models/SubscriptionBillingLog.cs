using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class SubscriptionBillingLog
{
    public int BillId { get; set; }

    public int? HostSubId { get; set; }

    public decimal? Amount { get; set; }

    public string? PaidStatus { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? Note { get; set; }

    public virtual HostSubscription? HostSub { get; set; }
}
