using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Booking
{
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

    public virtual ICollection<BookingGuest> BookingGuests { get; set; } = new List<BookingGuest>();

    public virtual Coupon? Coupon { get; set; }

    public virtual User? Guest { get; set; }

    public virtual ICollection<HostPayoutItem> HostPayoutItems { get; set; } = new List<HostPayoutItem>();

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Payment> Payments { get; set; } = new List<Payment>();

    public virtual ICollection<PointLedger> PointLedgers { get; set; } = new List<PointLedger>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual RoomList? Room { get; set; }
}
