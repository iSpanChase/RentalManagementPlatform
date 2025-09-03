using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class BookingGuest
{
    public int BookingGuestId { get; set; }

    public int? BookingId { get; set; }

    public string? GuestName { get; set; }

    public string? GuestIdNumber { get; set; }
}
