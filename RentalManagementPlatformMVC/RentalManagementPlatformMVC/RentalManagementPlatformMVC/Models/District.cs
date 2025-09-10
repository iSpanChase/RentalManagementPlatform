using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class District
{
    public int DistrictId { get; set; }

    public int? CityId { get; set; }

    public string? DistrictName { get; set; }
}
