using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public int? DistrictId { get; set; }

    public decimal Latitude { get; set; }

    public decimal Longitude { get; set; }

    public string? Street { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
