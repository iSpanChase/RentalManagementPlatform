using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public interface IUserRepository
	{
		Task<User?> GetByEmailAsync(string email);
		Task<User?> GetByProviderAsync(string provider, string subject);
		Task<User?> GetByIdAsync(int userId);
		Task AddAsync(User user);
		Task SaveChangesAsync();
		IQueryable<User> Query();
	}
}
