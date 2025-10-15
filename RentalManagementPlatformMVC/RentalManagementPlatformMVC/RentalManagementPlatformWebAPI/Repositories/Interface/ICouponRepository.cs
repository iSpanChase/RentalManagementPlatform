using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interface
{
	public interface ICouponRepository
	{
		Task<Coupon?> GetByIdAsync(int couponId);
	}
}
