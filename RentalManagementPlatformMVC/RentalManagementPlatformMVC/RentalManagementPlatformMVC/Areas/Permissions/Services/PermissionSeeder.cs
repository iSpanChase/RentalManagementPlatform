using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Permissions.Models;
using RentalManagementPlatformMVC.Models;
using System;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public class PermissionSeeder
	{
		public static async Task SeedAsync(RentalManagementPlatformSqlContext db)
		{
			var existing = await db.Set<Permission>()
								   .Select(p => p.PermCode)
								   .ToListAsync();
			var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

			var toAdd = AppPermissions.All()
				.Where(x => !set.Contains(x.code))
				.Select(x => new Permission
				{
					PermCode = x.code,
					PermName = x.name,
					Module = x.module,
					Action = x.action,
					Description = x.desc ?? "",
					CreatedAt = DateTime.UtcNow,
					UpdatedAt = DateTime.UtcNow
				}).ToList();

			if (toAdd.Count > 0)
			{
				db.AddRange(toAdd);
				await db.SaveChangesAsync();
			}
		}

		public static async Task GrantAllToAdminAsync(RentalManagementPlatformSqlContext db, int adminRoleId)
		{
			var allPids = await db.Set<Permission>().Select(p => p.PermissionId).ToListAsync();
			var owned = await db.Set<RolePermission>()
								.Where(rp => rp.RoleId == adminRoleId)
								.Select(rp => rp.PermissionId)
								.ToListAsync();
			var add = allPids.Except(owned).Select(pid => new RolePermission
			{
				RoleId = adminRoleId,
				PermissionId = pid,
				CreatedAt = DateTime.UtcNow
			});
			db.AddRange(add);
			await db.SaveChangesAsync();
		}
	}
}
