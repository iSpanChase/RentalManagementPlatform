using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	public interface IUserRepository : IRepository<UserEntity>
	{
		Task<UserEntity?> GetByUsernameAsync(string username);
		Task<bool> ExistsByUsernameAsync(string username);
		Task<bool> ExistsByEmailAsync(string email);
		IQueryable<UserEntity> Query();
	}
}
