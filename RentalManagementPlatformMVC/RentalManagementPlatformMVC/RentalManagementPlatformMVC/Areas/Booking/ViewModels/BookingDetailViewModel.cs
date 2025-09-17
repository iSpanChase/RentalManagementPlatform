using RentalManagementPlatformMVC.DTOs.Bookings;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
	public class BookingDetailViewModel
	{
		public int BookingId { get; set; }

		[Display(Name = "訂單編號")]
		public string? OrderNumber { get; set; }

		[Display(Name = "入住日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? CheckIn { get; set; }

		[Display(Name = "退房日期")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = false)]
		public DateTime? CheckOut { get; set; }

		[Display(Name = "總金額")]
		[DisplayFormat(DataFormatString = "NT$ {0:N0}", ApplyFormatInEditMode = false)]
		public decimal? TotalPrice { get; set; }

		[Display(Name = "平台抽成比例")]
		[DisplayFormat(DataFormatString = "{0:P2}")] // 例如 0.15 → 15.00%
		public decimal? CommissionRateSnapshot { get; set; }

		[Display(Name = "獲得點數")]
		public int? PointsEarned { get; set; }

		[Display(Name = "使用點數")]
		public int? PointsRedeemed { get; set; }

		[Display(Name = "狀態")]
		public string? Status { get; set; }

		[Display(Name = "建立時間")]
		[DisplayFormat(DataFormatString = "{0:yyyy-MM-dd HH:mm}", ApplyFormatInEditMode = false)]
		public DateTime? CreatedAt { get; set; }

		[Display(Name = "房客姓名")]
		public string? GuestName { get; set; }

		[Display(Name = "使用優惠券")]
		public string? Coupon { get; set; }

		[Display(Name = "房源名稱")]
		public string? Room { get; set; }

		[Display(Name = "房東姓名")]
		public string? HostName { get; set; }

		[Display(Name = "同行旅客")]
		public List<BookingGuestDto>? Guests { get; set; } = new();

		// 狀態顯示中文
		public string DisplayStatus => Status?.ToLower() switch
		{
			"confirmed" => "已確認",
			"pending" => "待確認",
			"cancelled" => "已取消",
			"completed" => "已完成",
			_ => "未知"
		};
	}
}
