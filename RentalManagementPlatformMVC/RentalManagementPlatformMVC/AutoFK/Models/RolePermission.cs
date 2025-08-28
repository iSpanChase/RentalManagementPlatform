using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class RolePermission
{
    public int RolePermissionId { get; set; }

    public int RoleId { get; set; }

    public int PermissionId { get; set; }

    public DateTime CreatedAt { get; set; }
}
