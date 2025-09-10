using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class RoomList
{
    public int RoomId { get; set; }

    public int? AddressId { get; set; }

    public int? HostId { get; set; }

    public string? Title { get; set; }

    public string? Description { get; set; }

    public int? MaxGuests { get; set; }

    public decimal? PricePerNight { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
