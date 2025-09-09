using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories;
using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	public class UserRepository : EfRepository<UserEntity>, IUserRepository
	{
		public UserRepository(RentalManagementPlatformSqlContext db) : base(db) { }

		// === IUserRepository 擴充方法 ===
		public Task<UserEntity?> GetByUsernameAsync(string username)
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

		public IQueryable<UserEntity> Query()
		{
			// 查清單/分頁時 NoTracking 效能較好
			return _db.Users.AsNoTracking();
		}
	}
}
