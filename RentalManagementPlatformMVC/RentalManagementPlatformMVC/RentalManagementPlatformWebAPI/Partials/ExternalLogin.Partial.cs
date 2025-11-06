using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class ExternalLogin
{
    public virtual User User { get; set; } = null!;
}
