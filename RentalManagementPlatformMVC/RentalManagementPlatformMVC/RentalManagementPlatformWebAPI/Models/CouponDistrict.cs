using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models;

public partial class CouponDistrict
{
    public int CouponDistrictId { get; set; }

    public int? CouponId { get; set; }

    public int? DistrictId { get; set; }
}
