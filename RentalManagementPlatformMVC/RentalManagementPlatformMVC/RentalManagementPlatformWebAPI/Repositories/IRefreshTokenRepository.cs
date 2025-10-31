using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public interface IRefreshTokenRepository
	{
		Task AddAsync(RefreshToken token);
		Task<RefreshToken?> GetAsync(string token);
		Task<List<RefreshToken>> GetActiveByUserAsync(int userId, DateTime nowUtc);
		Task SaveChangesAsync();
	}
}
