using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class UserRole
{
    public int UserRoleId { get; set; }

    public int UserId { get; set; }

    public int RoleId { get; set; }

    public DateTime CreatedAt { get; set; }
}
