using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	//repository介面:定義資料存取方法
	public interface ICouponGuestRepository
	{
		//回傳CouponGuest資料表IQueryable
		IQueryable<CouponGuest> Query();
	}
}
