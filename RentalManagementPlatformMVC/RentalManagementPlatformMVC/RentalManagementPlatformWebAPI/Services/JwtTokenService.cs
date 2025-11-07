using Microsoft.IdentityModel.Tokens;
using RentalManagementPlatformWebAPI.DTOs;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentalManagementPlatformWebAPI.Services
{
	public class JwtTokenService : IJwtTokenService
	{
		private readonly IConfiguration _cfg;
		private readonly IRoleService _roles;
		private readonly IPermissionService _perms;

		public JwtTokenService(IConfiguration cfg, IRoleService roles, IPermissionService perms)
		{
			_cfg = cfg;
			_roles = roles;
			_perms = perms;
		}

		public JwtPair Create(int userId, string email, string fullName, IEnumerable<string> roles, IEnumerable<string> permissions)
		{
			roles ??= Enumerable.Empty<string>();         // ★ null-safe
			permissions ??= Enumerable.Empty<string>();   // ★ null-safe

			var keyRaw = _cfg["Jwt:Key"];
			if (string.IsNullOrWhiteSpace(keyRaw))
				throw new InvalidOperationException("JWT Key 未設定（Jwt:Key）。");
			if (Encoding.UTF8.GetByteCount(keyRaw) < 32)
				throw new InvalidOperationException("JWT Key 長度不足（HS256 建議至少 32 bytes）。");

			var issuer = _cfg["Jwt:Issuer"];
			var audience = _cfg["Jwt:Audience"];
			if (string.IsNullOrWhiteSpace(issuer) || string.IsNullOrWhiteSpace(audience))
				throw new InvalidOperationException("Jwt:Issuer 或 Jwt:Audience 未設定。");

			var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(keyRaw));
			var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

			var claims = new List<Claim>
	{
		new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
		new Claim(ClaimTypes.Email, email ?? string.Empty),
		new Claim(ClaimTypes.Name,  fullName ?? string.Empty),
	};

			foreach (var r in roles.Distinct())
				claims.Add(new Claim(ClaimTypes.Role, r));

			foreach (var p in permissions.Distinct())
				claims.Add(new Claim("perm", p));

			var minutes = int.TryParse(_cfg["Jwt:AccessTokenMinutes"], out var m) ? m : 30;
			var expires = DateTime.UtcNow.AddMinutes(minutes);

			var token = new JwtSecurityToken(
				issuer: _cfg["Jwt:Issuer"],
				audience: _cfg["Jwt:Audience"],
				claims: claims,
				expires: expires,
				signingCredentials: creds
			);

			var access = new JwtSecurityTokenHandler().WriteToken(token);
			var refresh = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
			return new JwtPair(access, expires, refresh);
		}
		public Task<string> IssueTokenAsync(UserProfileDto user)
		{
			// 依你的 UserProfileDto 欄位對應：UserId / Email / Name / Username
			// 目前沒有可查「使用者角色/權限」的服務介面方法 → 先帶空集合
			var pair = Create(
				user.UserId,
				user.Email ?? string.Empty,
				string.IsNullOrWhiteSpace(user.Name) ? user.Username : user.Name,
				Enumerable.Empty<string>(),
				Enumerable.Empty<string>()
			);
			return Task.FromResult(pair.AccessToken);
		}
	}
}
