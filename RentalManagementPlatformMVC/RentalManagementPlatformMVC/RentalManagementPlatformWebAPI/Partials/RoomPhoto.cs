using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace RentalManagementPlatformWebAPI.Models;

public partial class RoomPhoto
{
    [ForeignKey("RoomId")]
    public virtual RoomList RoomList { get; set; }
}
