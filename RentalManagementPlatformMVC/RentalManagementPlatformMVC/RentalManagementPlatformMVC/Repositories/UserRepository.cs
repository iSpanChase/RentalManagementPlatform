using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public class UserRepository : EfRepository<User>, IUserRepository
	{
		public UserRepository(RentalManagementPlatformSqlContext db) : base(db) { }

		// === IUserRepository 擴充方法 ===
		public Task<User?> GetByUsernameAsync(string username)
		{
			return _db.Users.FirstOrDefaultAsync(u => u.Username == username);
		}

		public Task<bool> ExistsByUsernameAsync(string username)
		{
			return _db.Users.AnyAsync(u => u.Username == username);
		}

		public Task<bool> ExistsByEmailAsync(string email)
		{
			return _db.Users.AnyAsync(u => u.Email == email);
		}

		public IQueryable<User> Query()
		{
			// 查清單/分頁時 NoTracking 效能較好
			return _db.Users.AsNoTracking();
		}
	}
}
