using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repository.Interfaces;

namespace RentalManagementPlatformAPI.Repository
{
	public class UserReadRepository: IUserReadRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public UserReadRepository(RentalManagementPlatformSqlContext db) =>_db = db;//建構函式注入：讓外部提供 DbContext 實例

		public async Task<User?> GetByIdAsync(int userId)
		{
			return await _db.Set<User>()
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public IQueryable<User> Query()
		{
			return _db.Set<User>().AsQueryable();
		}
	}
}
