using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Models;
using RolesEntity = RentalManagementPlatformMVC.Models.Role;

namespace RentalManagementPlatformMVC.Areas.Roles.RolesRepositories
{
	public class RolesRepository : EfRepository<RolesEntity>, IRolesRepository
	{
		/// <summary>
		/// 以 DbContext 建立資料。
		/// </summary>
		public RolesRepository(RentalManagementPlatformSqlContext db) : base(db) { }

		// === IRolesRepository 擴充方法 ===
		public Task<RolesEntity?> GetByRoleNameAsync(string roleName)
		{
			return _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
		}

		public async Task<List<int>> GetUserIdsInRoleAsync(int roleId)
		{
			return await _db.UserRoles
				.Where(ur => ur.RoleId == roleId)
				.Select(ur => ur.UserId)
				.ToListAsync();
		}

		public async Task<List<int>> GetPermissionIdsInRoleAsync(int roleId)
		{
			return await _db.RolePermissions
				.Where(rp => rp.RoleId == roleId)
				.Select(rp => rp.PermissionId)
				.ToListAsync();
		}

		public Task<bool> ExistsByRoleNameAsync(string roleName)
		{
			return _db.Roles.AnyAsync(r => r.RoleName == roleName);
		}

		public Task<bool> ExistsByDescriptionAsync(string description)
		{
			return _db.Roles.AnyAsync(r => r.Description == description);
		}

		public Task<bool> ExistsByRoleCodeAsync(string roleCode)
		{
			return _db.Roles.AnyAsync(r => r.RoleCode == roleCode);
		}

		public IQueryable<RolesEntity> Query()
		{
			// 查清單/分頁時 NoTracking 效能較好
			return _db.Roles.AsNoTracking();
		}
	}
}
