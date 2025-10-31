
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories
{
    public class CouponDbRepository : ICouponDbRepository
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public CouponDbRepository(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

        public async Task<Coupon> GetCouponByCodeAsync(string code)
        {
            return await _context.Coupons.FirstOrDefaultAsync(c => c.DiscountCode == code && !c.IsDeleted);
        }

        public async Task<IEnumerable<CouponGuest>> GetUserCouponsByGuestIdAsync(int guestId)
        {
            // 使用 Include 來同時載入關聯的 Coupon 資料
            return await _context.CouponGuests
                .Include(cg => cg.Coupon)
                .Where(cg => cg.GuestId == guestId)
                .ToListAsync();
        }

        public async Task<CouponGuest> GetUserCouponAsync(int guestId, int couponId)
        {
            return await _context.CouponGuests
                .FirstOrDefaultAsync(cg => cg.GuestId == guestId && cg.CouponId == couponId);
        }

        public async Task AddUserCouponAsync(CouponGuest couponGuest)
        {
            await _context.CouponGuests.AddAsync(couponGuest);
        }

        public async Task<int> SaveChangesAsync()
        {
                return await _context.SaveChangesAsync();
        }

		                public async Task<IEnumerable<Coupon>> GetAllAsync()

		                {

		                    return await _context.Coupons.ToListAsync();

		                }    }
}
