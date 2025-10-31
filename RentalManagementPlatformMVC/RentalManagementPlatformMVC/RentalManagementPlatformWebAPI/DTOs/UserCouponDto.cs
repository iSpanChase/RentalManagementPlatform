
using System;

namespace RentalManagementPlatformWebAPI.DTOS
{
    public class UserCouponDto
    {
        public int CouponId { get; set; }
        public string CouponName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string DiscountCode { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime EndAt { get; set; }
        public DateTime StartAt { get; set; }
        public string DiscountMethod { get; set; } = string.Empty;
        public decimal DiscountQuota { get; set; }
        public decimal? LowSpend { get; set; }
    }
}
