using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class District
{
    public int DistrictId { get; set; }

    public int? CityId { get; set; }

    public string? DistrictName { get; set; }

    public virtual ICollection<Address> Addresses { get; set; } = new List<Address>();

    public virtual City? City { get; set; }

    public virtual ICollection<CouponDistrict> CouponDistricts { get; set; } = new List<CouponDistrict>();

    public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
}
