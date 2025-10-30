using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class RoomList
{
    public virtual ICollection<RoomPhoto> RoomPhotos { get; set; } = new List<RoomPhoto>();
    public virtual Address? Address { get; set; }
}
