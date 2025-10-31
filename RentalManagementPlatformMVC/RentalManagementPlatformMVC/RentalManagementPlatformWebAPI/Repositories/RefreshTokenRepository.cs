using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class RefreshTokenRepository : IRefreshTokenRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public RefreshTokenRepository(RentalManagementPlatformSqlContext db) { _db = db; }

		public async Task AddAsync(RefreshToken token)
		{
			_db.RefreshTokens.Add(token);
			await _db.SaveChangesAsync();
		}

		public Task<RefreshToken?> GetAsync(string token) =>
			_db.RefreshTokens.FirstOrDefaultAsync(t => t.Token == token);

		public Task<List<RefreshToken>> GetActiveByUserAsync(int userId, DateTime nowUtc) =>
			_db.RefreshTokens
			   .Where(t => t.UserId == userId && !t.Revoked && t.ExpiresAt > nowUtc)
			   .ToListAsync();

		public Task SaveChangesAsync() => _db.SaveChangesAsync();
	}
}
