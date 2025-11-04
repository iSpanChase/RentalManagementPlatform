using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Caching.Memory;
using System.Security.Claims;
using RentalManagementPlatformWebAPI.Repositories;
namespace RentalManagementPlatformWebAPI.Auth
{
	public class PermissionClaimsTransformation : IClaimsTransformation
	{
		private readonly IRoleRepository _roles;
		private readonly IPermissionRepository _perms;
		private readonly IMemoryCache _cache;
		private readonly ILogger<PermissionClaimsTransformation> _logger;

		public PermissionClaimsTransformation(
			IRoleRepository roles,
			IPermissionRepository perms,
			IMemoryCache cache,
			ILogger<PermissionClaimsTransformation> logger)
		{
			_roles = roles; _perms = perms; _cache = cache; _logger = logger;
		}

		public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
		{
			if (principal.Identity is not ClaimsIdentity id || !id.IsAuthenticated) return principal;

			// 避免重複堆疊
			if (id.HasClaim(c => c.Type == ClaimTypes.Role) && id.HasClaim(c => c.Type == "perm"))
				return principal;

			// ★ 這行改成用 principal 來取，而不是 id
			var sub =
				principal.FindFirstValue(ClaimTypes.NameIdentifier) ??
				principal.FindFirst("sub")?.Value ??
				principal.FindFirst("user_id")?.Value ??  // 你專案若有用這個
				principal.FindFirst("uid")?.Value;        // 其他可能別名

			if (!int.TryParse(sub, out var userId)) return principal;

			try
			{
				var cacheKey = $"auth:claims:{userId}";
				if (!_cache.TryGetValue(cacheKey, out (List<string> roles, List<string> perms) rp))
				{
					var roles = await _roles.GetCodesByUserIdAsync(userId);
					var perms = await _perms.GetCodesByUserIdAsync(userId);
					rp = (roles ?? new(), perms ?? new());
					_cache.Set(cacheKey, rp, TimeSpan.FromMinutes(5));
				}

				foreach (var r in rp.roles.Distinct())
					id.AddClaim(new Claim(ClaimTypes.Role, r));

				foreach (var p in rp.perms.Distinct())
					id.AddClaim(new Claim("perm", p));
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, "Failed to enrich claims for user {UserId}", userId);
				return principal; // 不讓它炸成 500
			}

			return principal;
		}
	}
}
