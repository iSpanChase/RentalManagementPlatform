using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class CouponGuest
{
    public int CouponGuestId { get; set; }

    public int? CouponId { get; set; }

    public int? GuestId { get; set; }

    public DateTime? CreateAt { get; set; }

    public DateTime? RemoveAt { get; set; }
}
