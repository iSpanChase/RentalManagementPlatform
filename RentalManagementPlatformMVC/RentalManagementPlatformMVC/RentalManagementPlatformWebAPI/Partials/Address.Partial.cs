using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class Address
{
    public virtual District? District { get; set; }
}
