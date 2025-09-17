//repository層
//CouponGuest的資料查詢(只讀查詢)
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository
{
	public class CouponGuestRepository : ICouponGuestRepository
	{
		public readonly RentalManagementPlatformSqlContext _db;
		//建構子注入dbcontext
		public CouponGuestRepository(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}
		//提供只讀查詢,使用AsNoTracking只讀資料，不需要 EF 追蹤，提高效能
		public IQueryable<CouponGuest> Query() 
		{ 
			return _db.CouponGuests.AsNoTracking();
		}
	}
}
