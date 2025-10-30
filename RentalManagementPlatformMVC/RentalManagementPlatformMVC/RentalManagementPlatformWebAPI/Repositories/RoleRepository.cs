using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class RoleRepository : IRoleRepository
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public RoleRepository(RentalManagementPlatformSqlContext db) => _db = db;

		public Task<Role?> GetByIdAsync(int roleId) =>
			_db.Roles.FirstOrDefaultAsync(r => r.RoleId == roleId);

		public Task<Role?> GetByCodeAsync(string roleCode) =>
			_db.Roles.FirstOrDefaultAsync(r => r.RoleCode == roleCode);

		public Task<List<Role>> GetAllAsync() =>
			_db.Roles.OrderBy(r => r.RoleCode).ToListAsync();

		public async Task AssignUserAsync(int roleId, int userId)
		{
			var exists = await _db.UserRoles
				.AnyAsync(x => x.RoleId == roleId && x.UserId == userId);
			if (!exists)
			{
				_db.UserRoles.Add(new UserRole { RoleId = roleId, UserId = userId });
				await _db.SaveChangesAsync();
			}
		}

		public async Task RevokeUserAsync(int roleId, int userId)
		{
			var ur = await _db.UserRoles
				.FirstOrDefaultAsync(x => x.RoleId == roleId && x.UserId == userId);
			if (ur != null)
			{
				_db.UserRoles.Remove(ur);
				await _db.SaveChangesAsync();
			}
		}

		public Task<bool> HasUsersAsync(int roleId) =>
			_db.UserRoles.AnyAsync(x => x.RoleId == roleId);

		public Task<List<string>> GetCodesByUserIdAsync(int userId) => _db.UserRoles
			.Where(ur => ur.UserId == userId)
			.Select(ur => ur.Role.RoleCode)
			.Distinct()
			.ToListAsync();

		public Task<List<int>> GetUserIdsByRoleIdAsync(int roleId) =>_db.UserRoles
			.Where(ur => ur.RoleId == roleId)
			.Select(ur => ur.UserId)
			.Distinct()
			.ToListAsync();
	}
}
