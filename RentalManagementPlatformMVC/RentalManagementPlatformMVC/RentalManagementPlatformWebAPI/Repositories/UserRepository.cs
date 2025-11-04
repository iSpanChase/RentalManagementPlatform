using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class UserRepository : IUserRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public UserRepository(RentalManagementPlatformSqlContext db) { _db = db; }

		public Task<User?> GetByIdAsync(int userId) =>
			_db.Users.FirstOrDefaultAsync(u => u.UserId == userId);

		public Task<User?> GetByEmailAsync(string email) =>
			_db.Users.FirstOrDefaultAsync(u => u.Email == email);

		public Task<User?> GetByUsernameAsync(string username) =>
			_db.Users.FirstOrDefaultAsync(u => u.Username == username);

		public Task<User?> GetByProviderAsync(string provider, string subject) =>
			_db.Users.FirstOrDefaultAsync(u => u.Provider == provider && u.ProviderSubject == subject);

		public async Task AddAsync(User user)
		{
			_db.Users.Add(user);
			await _db.SaveChangesAsync();
		}

		public Task SaveChangesAsync() => _db.SaveChangesAsync();

		public IQueryable<User> Query() => _db.Users.AsQueryable();
	}
}
