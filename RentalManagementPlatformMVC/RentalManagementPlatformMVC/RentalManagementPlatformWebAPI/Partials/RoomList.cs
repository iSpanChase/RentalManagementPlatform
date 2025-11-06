using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementPlatformWebAPI.Models;

public partial class RoomList
{
    public virtual ICollection<RoomPhoto> RoomPhotos { get; set; } = new List<RoomPhoto>();
    public virtual Address? Address { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public User? Host { get; set; }
}

