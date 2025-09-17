//repository層
//District的資料查詢(只讀查詢)
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository
{
	public class DistrictRepository: IDistrictRepository
	{
		public readonly RentalManagementPlatformSqlContext _db;
		//建構子注入dbcontext
		public DistrictRepository(RentalManagementPlatformSqlContext db) 
		{
			_db = db;
		}
		//提供只讀查詢,使用AsNoTracking只讀資料
		public IQueryable<District> Query() 
		{
			return _db.Districts.AsNoTracking();
		}
	}
}
