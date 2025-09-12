using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class SubscriptionPlan
{
	public int PlanId { get; set; }

	// 必填欄位 - 移除可空型別
	public string PlanName { get; set; } = string.Empty;
	public decimal MonthlyFee { get; set; }
	public decimal CommissionRate { get; set; }
	public DateTime CreatedAt { get; set; }

	// 布林值預設為 false - 移除可空型別
	public bool PerkPriority { get; set; }
	public bool PerkAnalytics { get; set; }
	public bool IsActive { get; set; }
}
