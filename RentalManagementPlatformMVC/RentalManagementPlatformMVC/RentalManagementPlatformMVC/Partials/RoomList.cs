using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class RoomList
{
    public virtual ICollection<RoomPhoto> RoomPhotos { get; set; } = new List<RoomPhoto>();
}
