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
			// 取 DB 目前已有的 perm_code
			var existing = await db.Permissions
								   .Select(p => p.PermCode)
								   .ToListAsync();
			var set = existing.ToHashSet(StringComparer.OrdinalIgnoreCase);

			// 逐一比對常數清單，把不存在的插進去
			var toAdd = AppPermissions.AllCodes()
									  .Where(code => !set.Contains(code))
									  .Select(code => new Permission
									  {
										  PermCode = code,
										  PermName = code,   // 如果你有對應顯示名稱，可以再改
										  Module = code.Split('.')[0],
										  Action = code.Split('.')[1],
										  Description = "",
										  CreatedAt = DateTime.UtcNow,
										  UpdatedAt = DateTime.UtcNow
									  })
									  .ToList();

			if (toAdd.Count > 0)
			{
				db.Permissions.AddRange(toAdd);
				await db.SaveChangesAsync();
			}

			// 確保有「Admin 角色」並賦予所有權限
			var adminRole = await db.Roles.FirstOrDefaultAsync(r => r.RoleCode == "ADMIN");
			if (adminRole != null)
			{
				var existingPerms = await db.RolePermissions
											.Where(rp => rp.RoleId == adminRole.RoleId)
											.Select(rp => rp.PermissionId)
											.ToListAsync();
				var missingPermIds = await db.Permissions
											 .Where(p => !existingPerms.Contains(p.PermissionId))
											 .Select(p => p.PermissionId)
											 .ToListAsync();

				foreach (var pid in missingPermIds)
				{
					db.RolePermissions.Add(new RolePermission
					{
						RoleId = adminRole.RoleId,
						PermissionId = pid,
						CreatedAt = DateTime.UtcNow
					});
				}
				if (missingPermIds.Any())
					await db.SaveChangesAsync();
			}
		}
	}
}
