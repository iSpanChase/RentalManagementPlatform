using System.Linq.Expressions;

namespace RentalManagementPlatformMVC.Repositories
{
	/// <summary>
	/// 泛型資料介面。提供最基本的新增/更新/刪除與查詢能力。
	/// </summary>
	/// <typeparam name="T">實體類型。</typeparam>
	public interface IRepository<T> where T : class
	{
		/// <summary>以主鍵取得實體資料（可能為 null）。</summary>
		Task<T?> GetByIdAsync(object id);
		Task<IReadOnlyList<T>> ListAsync(Expression<Func<T, bool>>? predicate = null);

		/// <summary>
		/// 新增一筆實體資料（不提交）。
		/// </summary>
		Task AddAsync(T entity);

		/// <summary>
		/// 標記更新一筆實體資料（不提交）。
		/// </summary>
		void Update(T entity);

		/// <summary>
		/// 標記刪除一筆實體資料（不提交）。
		/// </summary>
		void Remove(T entity);
	}
}
