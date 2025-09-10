using System;
using System.Collections.Generic;

namespace AutoFK.Models;

public partial class CouponDistrict
{
    public int CouponDistrictId { get; set; }

    public int? CouponId { get; set; }

    public int? DistrictId { get; set; }
}
