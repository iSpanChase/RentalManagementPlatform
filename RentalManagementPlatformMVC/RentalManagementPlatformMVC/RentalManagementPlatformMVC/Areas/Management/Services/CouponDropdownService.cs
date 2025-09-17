using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class CouponDropdownService
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public CouponDropdownService(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}
		public async Task<List<Coupon>> GetAvailableCouponsAsync() 
		{
			var now = DateTime.Now;
			return await _db.Coupons
				//排除已刪除,可以無期限,未過期的優惠券
				.Where(c => !c.IsDeleted && (c.EndAt == null || c.EndAt > now))
				.ToListAsync();
		}
	}
}
