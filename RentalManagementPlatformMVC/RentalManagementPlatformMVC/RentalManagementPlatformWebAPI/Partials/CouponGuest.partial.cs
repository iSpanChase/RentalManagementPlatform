
using System;
using System.Collections.Generic;

namespace RentalManagementPlatformWebAPI.Models
{
    // 使用 partial 關鍵字，讓這個檔案作為 Models/CouponGuest.cs 的擴充
    public partial class CouponGuest
    {
        // Entity Framework Core 會根據 CouponId 這個外鍵，將這個屬性與 Coupon 資料表關聯起來
        public virtual Coupon Coupon { get; set; } = null!;

        // Entity Framework Core 會根據 GuestId 這個外鍵，將這個屬性與 User 資料表關聯起來
        public virtual User Guest { get; set; } = null!;
    }
}
