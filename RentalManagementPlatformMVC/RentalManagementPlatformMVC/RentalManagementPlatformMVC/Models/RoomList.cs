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

    public virtual Address? Address { get; set; }

    //public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    //public virtual User? Host { get; set; }

    public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    public virtual ICollection<Mongodb> Mongodbs { get; set; } = new List<Mongodb>();

    public virtual ICollection<Review> Reviews { get; set; } = new List<Review>();

    public virtual ICollection<RoomPhoto> RoomPhotos { get; set; } = new List<RoomPhoto>();
}
