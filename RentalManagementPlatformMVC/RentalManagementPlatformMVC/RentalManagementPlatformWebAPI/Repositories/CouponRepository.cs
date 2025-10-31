using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class CouponRepository : ICouponRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public CouponRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		public async Task<Coupon?> GetByIdAsync(int couponId)
		{
			return await _context.Coupons
				.FirstOrDefaultAsync(c => c.CouponId == couponId);
		}
	}
}