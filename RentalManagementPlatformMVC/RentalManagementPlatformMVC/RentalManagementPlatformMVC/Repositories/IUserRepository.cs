using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public interface IUserRepository : IRepository<User>
	{
		Task<User?> GetByUsernameAsync(string username);
		Task<bool> ExistsByUsernameAsync(string username);
		Task<bool> ExistsByEmailAsync(string email);
		IQueryable<User> Query();
	}
}
