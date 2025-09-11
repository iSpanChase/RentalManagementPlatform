using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	/// <summary>
	/// EF Core 工作單元實作。
	/// </summary>
	public class UnitOfWork : IUnitOfWork
	{
		private readonly RentalManagementPlatformSqlContext _db;
		/// <summary>
		/// 以 DbContext 建立工作單元。
		/// </summary>
		public UnitOfWork(RentalManagementPlatformSqlContext db) => _db = db;
		//public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);

		public async Task<int> SaveChangesAsync(CancellationToken ct = default)
		{
			var affected = await _db.SaveChangesAsync(ct);
			Console.WriteLine($"[UoW] SaveChanges affected = {affected}");
			return affected;
		}
	}
}
