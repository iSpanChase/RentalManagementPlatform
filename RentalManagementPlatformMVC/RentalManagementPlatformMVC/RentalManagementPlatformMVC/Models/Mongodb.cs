using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class Mongodb
{
    public string MongodbId { get; set; } = null!;

    public int ListingId { get; set; }

    public string? Description { get; set; }

    public string? ImageUrl { get; set; }

    public virtual RoomList Listing { get; set; } = null!;
}
