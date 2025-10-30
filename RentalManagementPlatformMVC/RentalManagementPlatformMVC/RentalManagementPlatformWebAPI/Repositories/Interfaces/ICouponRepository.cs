using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface ICouponRepository
	{
		Task<Coupon?> GetByIdAsync(int couponId);
	}
}
