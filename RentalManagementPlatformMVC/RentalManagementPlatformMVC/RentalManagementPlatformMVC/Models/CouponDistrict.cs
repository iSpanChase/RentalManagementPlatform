using System;
using System.Collections.Generic;

namespace RentalManagementPlatformMVC.Models;

public partial class CouponDistrict
{
    public int CouponDistrictId { get; set; }

    public int? CouponId { get; set; }

    public int? DistrictId { get; set; }

    public virtual Coupon? Coupon { get; set; }

    public virtual District? District { get; set; }
}
