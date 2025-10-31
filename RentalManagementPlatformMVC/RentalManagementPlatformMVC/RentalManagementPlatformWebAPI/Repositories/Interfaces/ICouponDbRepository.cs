
using RentalManagementPlatformWebAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
    public interface ICouponDbRepository
    {
        Task<Coupon?> GetCouponByCodeAsync(string code);
        Task<IEnumerable<CouponGuest>> GetUserCouponsByGuestIdAsync(int guestId);

        Task<CouponGuest> GetUserCouponAsync(int guestId, int couponId);

        Task AddUserCouponAsync(CouponGuest couponGuest);

        Task<int> SaveChangesAsync();
		Task<IEnumerable<Coupon>> GetAllAsync();
	}
}
