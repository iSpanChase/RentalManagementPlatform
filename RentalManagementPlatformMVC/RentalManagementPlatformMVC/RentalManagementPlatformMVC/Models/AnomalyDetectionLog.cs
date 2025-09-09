using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class AnomalyDetectionLog
{
    public int LogId { get; set; }

    public int? RuleId { get; set; }

    public int? TargetId { get; set; }

    public decimal? DetectedValue { get; set; }

    public decimal? ExpectedValue { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual AnomalyRule? Rule { get; set; }
}
