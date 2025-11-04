using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

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

    public int? GuestCount { get; set; }

    public string? PaymentTiming { get; set; }

    public string? PaymentStatus { get; set; }

    public DateTime? PaymentDeadline { get; set; }

    public string? ContactName { get; set; }

    public string? ContactEmail { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactNotes { get; set; }

    public string? BillingCountry { get; set; }

    public string? BillingStreet { get; set; }

    public string? BillingApartment { get; set; }

    public string? BillingCity { get; set; }

    public string? BillingState { get; set; }

    public string? BillingZipCode { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
