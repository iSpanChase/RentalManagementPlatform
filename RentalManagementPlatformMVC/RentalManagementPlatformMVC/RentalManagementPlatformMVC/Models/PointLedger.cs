using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class PointLedger
{
    public int LedgerId { get; set; }

    public int? GuestId { get; set; }

    public int? BookingId { get; set; }

    public string? OrderNumberSnapshot { get; set; }

    public string? Type { get; set; }

    public int? Points { get; set; }

    public DateTime? OccurredAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public string? Note { get; set; }
}
