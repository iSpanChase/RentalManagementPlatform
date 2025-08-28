using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class SubscriptionPlan
{
    public int PlanId { get; set; }

    public string? PlanName { get; set; }

    public decimal? MonthlyFee { get; set; }

    public decimal? CommissionRate { get; set; }

    public bool? PerkPriority { get; set; }

    public bool? PerkAnalytics { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
