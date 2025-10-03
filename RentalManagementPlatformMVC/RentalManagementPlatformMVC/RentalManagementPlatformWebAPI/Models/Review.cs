using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Review
{
    public int ReviewId { get; set; }

    public int? BookingId { get; set; }

    public int? ReviewerId { get; set; }

    public int? RoomId { get; set; }

    public string? Comment { get; set; }

    public int? Rating { get; set; }

    public DateTime? CreatedAt { get; set; }
}
