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
		public CouponGuestRepository(RentalManagementPlatformSqlContext db)
		{
			_db = db;
		}

		public IQueryable<CouponGuest> Query() 
		{ 
			return _db.CouponGuests.AsNoTracking();
		}
	}
}
