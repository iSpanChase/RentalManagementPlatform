using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Coupon
{
    public int CouponId { get; set; }

    public string? CouponName { get; set; }

    public string? DiscountCode { get; set; }

    public string? Description { get; set; }

    public int? MinRentalPeriod { get; set; }

    public int? MaxRentalPeriod { get; set; }

    public int? StartRentalPeriod { get; set; }

    public int? EndRentalPeriod { get; set; }

    public string? DiscountMethod { get; set; }

    public decimal? DiscountQuota { get; set; }

    public DateTime? EndAt { get; set; }

    public decimal? LowSpend { get; set; }

    public DateTime? StartAt { get; set; }

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<CouponDistrict> CouponDistricts { get; set; } = new List<CouponDistrict>();

    public virtual ICollection<CouponGuest> CouponGuests { get; set; } = new List<CouponGuest>();
}
