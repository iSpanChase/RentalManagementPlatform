using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public UnitOfWork(RentalManagementPlatformSqlContext db) => _db = db;
		public Task<int> SaveChangesAsync(CancellationToken ct = default) => _db.SaveChangesAsync(ct);
	}
}
