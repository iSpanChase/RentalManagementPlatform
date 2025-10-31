using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class District
{
    public virtual City? City { get; set; }
}
