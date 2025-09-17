using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Repository
{
	public class UserReadRepository: IUserReadRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public UserReadRepository(RentalManagementPlatformSqlContext db) =>_db = db;//建構函式注入：讓外部提供 DbContext 實例

		public async Task<Models.User?> GetByIdAsync(int userId)
		{
			return await _db.Set<Models.User>()
				.FirstOrDefaultAsync(u => u.UserId == userId);
		}

		public IQueryable<Models.User> Query()
		{
			return _db.Set<Models.User>().AsQueryable();
		}
	}
}
