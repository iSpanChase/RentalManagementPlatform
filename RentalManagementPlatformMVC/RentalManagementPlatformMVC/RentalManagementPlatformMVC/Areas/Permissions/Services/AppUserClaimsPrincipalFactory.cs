using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using RentalManagementPlatformMVC.Models;
using System;
using System.Security.Claims;
using AppUser = RentalManagementPlatformMVC.Models.User;
using AppRole = RentalManagementPlatformMVC.Models.Role;
using Microsoft.EntityFrameworkCore;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public class AppUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<AppUser, AppRole>
	{
		private readonly RentalManagementPlatformSqlContext _db;
		public AppUserClaimsPrincipalFactory(
			UserManager<AppUser> um, RoleManager<AppRole> rm, IOptions<IdentityOptions> opt, RentalManagementPlatformSqlContext db)
			: base(um, rm, opt) => _db = db;

		protected override async Task<ClaimsIdentity> GenerateClaimsAsync(AppUser user)
		{
			var id = await base.GenerateClaimsAsync(user);
			var roles = await UserManager.GetRolesAsync(user);

			var codes = await (from r in _db.Set<AppRole>()
							   where roles.Contains(r.RoleName)
							   from rp in r.RolePermissions
							   join p in _db.Set<Permission>() on rp.PermissionId equals p.PermissionId
							   select p.PermCode).Distinct().ToListAsync();

			foreach (var c in codes) id.AddClaim(new Claim("permission", c));
			return id;
		}
	}
}
