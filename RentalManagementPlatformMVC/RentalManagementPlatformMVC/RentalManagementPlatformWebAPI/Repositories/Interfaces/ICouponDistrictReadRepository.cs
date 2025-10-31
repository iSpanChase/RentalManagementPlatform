//Repository層 (收尋CouponDistrict資料的介面)
//用於servics判斷地區的商業邏輯
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repository.Interfaces
{
	public interface ICouponDistrictReadRepository//定義CouponDistrict的Interface => ICouponDistrictReadRepository
	{
		IQueryable<CouponDistrict> Query();//查詢CoupponDistrict的資料,由service層決定判斷
	}
}
