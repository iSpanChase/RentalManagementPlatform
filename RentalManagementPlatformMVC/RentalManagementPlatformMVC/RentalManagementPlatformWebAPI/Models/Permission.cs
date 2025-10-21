using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class Permission
{
    public int PermissionId { get; set; }

    public string PermCode { get; set; } = null!;

    public string PermName { get; set; } = null!;

    public string Module { get; set; } = null!;

    public string Action { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
