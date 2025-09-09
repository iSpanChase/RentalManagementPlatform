using System.Linq.Expressions;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	public interface IRepository<T> where T : class
	{
		Task<T?> GetByIdAsync(object id);
		Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);
		Task AddAsync(T entity);
		void Update(T entity);
		void Remove(T entity);
	}
}
