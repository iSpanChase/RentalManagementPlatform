namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken ct = default);
	}
}
