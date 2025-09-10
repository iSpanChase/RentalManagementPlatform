using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class RoomPhoto
{
    public int PhotoId { get; set; }

    public int? RoomId { get; set; }

    public int? SortOrder { get; set; }

    public string Bucket { get; set; } = null!;

    public string ObjectKey { get; set; } = null!;

    public string ContentType { get; set; } = null!;
}
