using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Models;
using PermissionEntity = RentalManagementPlatformMVC.Models.Permission;

namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsRepositories
{
	public class PermissionsRepository : EfRepository<PermissionEntity>, IPermissionsRepository
	{
		public PermissionsRepository(RentalManagementPlatformSqlContext db) : base(db) { }

		public IQueryable<PermissionEntity> Query()
			=> _db.Permissions.AsNoTracking();

		public Task<bool> ExistsByCodeAsync(string permCode, int? excludeId = null)
		{
			var q = _db.Permissions.AsQueryable().Where(p => p.PermCode == permCode);
			if (excludeId.HasValue) q = q.Where(p => p.PermissionId != excludeId.Value);
			return q.AnyAsync();
		}

		public Task<bool> ExistsByModuleActionAsync(string module, string action, int? excludeId = null)
		{
			var q = _db.Permissions.AsQueryable().Where(p => p.Module == module && p.Action == action);
			if (excludeId.HasValue) q = q.Where(p => p.PermissionId != excludeId.Value);
			return q.AnyAsync();
		}
	}
}
