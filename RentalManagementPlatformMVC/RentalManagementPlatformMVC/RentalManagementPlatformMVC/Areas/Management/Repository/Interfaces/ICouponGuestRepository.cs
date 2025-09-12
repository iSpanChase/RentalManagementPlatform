//定義CouponGuest的Repository層ICouponGuestReadRepository介面
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	public interface ICouponGuestRepository
	{
		IQueryable <CouponGuest> Query();
	}
}
