using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class UserFavoriteReport
{
    public int FavoriteId { get; set; }

    public int? UserId { get; set; }

    public string? ReportType { get; set; }

    public string? ReportParams { get; set; }

    public DateTime? CreatedAt { get; set; }
}
