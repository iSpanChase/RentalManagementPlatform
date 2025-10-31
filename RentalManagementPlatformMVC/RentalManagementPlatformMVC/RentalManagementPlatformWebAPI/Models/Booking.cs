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

	// 住宿人數
	public int? GuestCount { get; set; }

	//付款時機 ('full' = 立即支付, 'partial' = 延後支付)
	public string? PaymentTiming { get; set; }

	// 付款狀態 ('paid' = 已付款, 'unpaid' = 未付款, 'refunded' = 已退款)
	public string? PaymentStatus { get; set; }

	// 付款截止日期 (僅在 PaymentTiming 為 'partial' 時適用)
	public DateTime? PaymentDeadline { get; set; }

	// --- 聯絡人/付款人資訊 ---

	// 聯絡人姓名
	public string? ContactName { get; set; }

	// 聯絡人 Email
	public string? ContactEmail { get; set; }

	// 聯絡人電話
	public string? ContactPhone { get; set; }

	// 特殊需求備註
	public string? ContactNotes { get; set; }

	// --- 帳單地址 ---

	// 國家代碼 (例如: TW)
	public string? BillingCountry { get; set; }

	// 街道地址
	public string? BillingStreet { get; set; }

	// 公寓/套房號碼
	public string? BillingApartment { get; set; }

	// 城市
	public string? BillingCity { get; set; }

	// 省份/州
	public string? BillingState { get; set; }

	// 郵遞區號
	public string? BillingZipCode { get; set; }

	// 更新日期
	public DateTime? UpdatedAt { get; set; }

}