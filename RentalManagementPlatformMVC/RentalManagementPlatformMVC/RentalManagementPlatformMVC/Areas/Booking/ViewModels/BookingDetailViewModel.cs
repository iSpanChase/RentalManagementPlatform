using System;
using System.Collections.Generic;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Areas.Booking.ViewModels
{
    public class BookingDetailViewModel
    {
        public int BookingId { get; set; }
        public string OrderNumber { get; set; }
        public DateTime? CheckIn { get; set; }
        public DateTime? CheckOut { get; set; }
        public decimal? TotalPrice { get; set; }
        public decimal? CommissionRateSnapshot { get; set; }
        public int? PointsEarned { get; set; }
        public int? PointsRedeemed { get; set; }
        public string Status { get; set; }
        public DateTime? CreatedAt { get; set; }
        public int? CouponId { get; set; }
        public int? GuestId { get; set; }
        public int? RoomId { get; set; }
        public List<BookingGuestDto> Guests { get; set; } = new();
		public string DisplayStatus => Status?.ToLower() switch
		{
			"confirmed" => "已確認",
			"pending" => "待確認",
			"cancelled" => "已取消",
			_ => "未知"
		};
	}
}
