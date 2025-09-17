using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class RolePermission
{
    public Role Role { get; set; } = null!;
    public Permission Permission { get; set; } = null!;
}
