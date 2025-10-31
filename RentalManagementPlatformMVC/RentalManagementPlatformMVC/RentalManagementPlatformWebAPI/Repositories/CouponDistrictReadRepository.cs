using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repository.Interfaces;

namespace RentalManagementPlatformAPI.Repository
{
	public class CouponDistrictReadRepository : ICouponDistrictReadRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;
		public CouponDistrictReadRepository(RentalManagementPlatformSqlContext context) 
		{
			_context = context;
		}

		public IQueryable<CouponDistrict> Query() 
		{
			return _context.CouponDistricts.AsQueryable();
		}

	}
}
