using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementPlatformWebAPI.Models;

public partial class Booking
{
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public int BookingId { get; set; }

	public int? CouponId { get; set; }

	public int? GuestId { get; set; }

	public int? RoomId { get; set; }

	public string? OrderNumber { get; set; }

	public DateTime? CheckIn { get; set; }

	public DateTime? CheckOut { get; set; }

	public decimal? TotalPrice { get; set; }

	public decimal? CommissionRateSnapshot { get; set; }

	public int? PointsEarned { get; set; }

	public int? PointsRedeemed { get; set; }

	public string? Status { get; set; }

	public DateTime? CreatedAt { get; set; }

	// ==================== 新增欄位 ====================

	/// <summary>
	/// 住宿人數
	/// </summary>
	public int? GuestCount { get; set; }

	/// <summary>
	/// 付款時機 ('full' = 立即支付, 'partial' = 延後支付)
	/// </summary>
	public string? PaymentTiming { get; set; }

	// --- 聯絡人/付款人資訊 ---

	/// <summary>
	/// 聯絡人姓名
	/// </summary>
	public string? ContactName { get; set; }

	/// <summary>
	/// 聯絡人 Email
	/// </summary>
	public string? ContactEmail { get; set; }

	/// <summary>
	/// 聯絡人電話
	/// </summary>
	public string? ContactPhone { get; set; }

	/// <summary>
	/// 特殊需求備註
	/// </summary>
	public string? ContactNotes { get; set; }

	// --- 帳單地址 ---

	/// <summary>
	/// 國家代碼 (例如: TW)
	/// </summary>
	public string? BillingCountry { get; set; }

	/// <summary>
	/// 街道地址
	/// </summary>
	public string? BillingStreet { get; set; }

	/// <summary>
	/// 公寓/套房號碼
	/// </summary>
	public string? BillingApartment { get; set; }

	/// <summary>
	/// 城市
	/// </summary>
	public string? BillingCity { get; set; }

	/// <summary>
	/// 省份/州
	/// </summary>
	public string? BillingState { get; set; }

	/// <summary>
	/// 郵遞區號
	/// </summary>
	public string? BillingZipCode { get; set; }
}