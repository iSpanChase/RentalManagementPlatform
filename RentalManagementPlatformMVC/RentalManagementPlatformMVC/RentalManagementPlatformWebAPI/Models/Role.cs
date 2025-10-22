using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RoleCode { get; set; } = null!;

    public string RoleName { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
