using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class AnomalyRule
{
    public int RuleId { get; set; }

    public string? RuleName { get; set; }

    public string? TargetType { get; set; }

    public string? ConditionExpression { get; set; }

    public decimal? ThresholdValue { get; set; }

    public bool? IsActive { get; set; }

    public DateTime? CreatedAt { get; set; }
}
