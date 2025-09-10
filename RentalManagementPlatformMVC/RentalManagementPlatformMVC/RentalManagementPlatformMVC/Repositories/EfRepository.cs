using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	/// <summary>
	/// EF Core 泛型資料基底實作。
	/// </summary>
	/// <typeparam name="T">實體類型。</typeparam>
	public class EfRepository<T> : IRepository<T> where T : class
	{
		/// <summary>供子類使用的 DbContext。</summary>
		protected readonly RentalManagementPlatformSqlContext _db;

		/// <summary>
		/// 以 DbContext 建立泛型資料。
		/// </summary>
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
