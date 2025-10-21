using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class PermissionRepository : IPermissionRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public PermissionRepository(RentalManagementPlatformSqlContext db) { _db = db; }

		public Task<List<Permission>> GetAllAsync() => _db.Permissions.OrderBy(p => p.PermCode).ToListAsync();

		public async Task AssignToRoleAsync(int roleId, IEnumerable<int> permissionIds)
		{
			var toAdd = permissionIds.Distinct().ToList();
			var existing = await _db.RolePermissions
				.Where(rp => rp.RoleId == roleId && toAdd.Contains(rp.PermissionId))
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			foreach (var pid in toAdd.Except(existing))
				_db.RolePermissions.Add(new RolePermission { RoleId = roleId, PermissionId = pid });

			await _db.SaveChangesAsync();
		}

		public async Task RemoveFromRoleAsync(int roleId, IEnumerable<int> permissionIds)
		{
			var items = await _db.RolePermissions
				.Where(rp => rp.RoleId == roleId && permissionIds.Contains(rp.PermissionId))
				.ToListAsync();

			_db.RolePermissions.RemoveRange(items);
			await _db.SaveChangesAsync();
		}

		public Task<List<string>> GetCodesByUserIdAsync(int userId) =>
			_db.UserRoles
				.Where(ur => ur.UserId == userId)
				.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermCode))
				.Distinct()
				.ToListAsync();
	}
}
