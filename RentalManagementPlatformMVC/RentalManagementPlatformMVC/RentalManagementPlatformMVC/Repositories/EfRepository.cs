using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public class EfRepository<T> : IRepository<T> where T : class
	{
		protected readonly RentalManagementPlatformSqlContext _db;
		public EfRepository(RentalManagementPlatformSqlContext db) => _db = db;

		public async Task<T?> GetByIdAsync(object id) => await _db.Set<T>().FindAsync(id);

		public async Task<IReadOnlyList<T>> ListAsync(System.Linq.Expressions.Expression<Func<T, bool>>? predicate = null)
			=> predicate is null
				? await _db.Set<T>().AsNoTracking().ToListAsync()
				: await _db.Set<T>().AsNoTracking().Where(predicate).ToListAsync();

		public Task AddAsync(T entity)
		{
			_db.Set<T>().Add(entity);
			return Task.CompletedTask; // SaveChanges 交給 UoW
		}

		public void Update(T entity) => _db.Set<T>().Update(entity);
		public void Remove(T entity) => _db.Set<T>().Remove(entity);
	}
}
