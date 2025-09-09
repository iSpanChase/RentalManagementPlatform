//DTO層
//功能:在Service層與Controller層之間傳遞資料
//用途:
//1.避免直接EF傳到View,提高安全性(不會洩漏敏感欄位或資料庫結構)
//2.只包含前端需要的欄位,減少資料傳輸量

using System;
namespace RentalManagementPlatformMVC.Areas.Management.DTOS;

public class CouponDto
{
	public int CouponId { get; set; }
	public string? CouponName { get; set; }
	public string? DiscountCode { get; set; }
	public string? Description { get; set; }
	public int? MinRentalPeriod { get; set; }
	public int? MaxRentalPeriod { get; set; }
	public DateTime? StartRentalPeriod { get; set; }
	public DateTime? EndRentalPeriod { get; set; }
	public string? DiscountMethod { get; set; }
	public decimal? DiscountQuota { get; set; }
	public decimal? LowSpend { get; set; }
	public DateTime? EndAt { get; set; }
	public bool IsDeleted { get; set; } // 是否已刪除 (軟刪除)
}

