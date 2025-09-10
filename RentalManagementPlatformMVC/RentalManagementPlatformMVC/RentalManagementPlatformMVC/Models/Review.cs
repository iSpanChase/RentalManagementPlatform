using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? BookingId { get; set; }

    public int? HostId { get; set; }

    public int? ReviewerId { get; set; }

    public int? RoomId { get; set; }

    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual User? Host { get; set; }

    public virtual User? Reviewer { get; set; }

    public virtual RoomList? Room { get; set; }
}
