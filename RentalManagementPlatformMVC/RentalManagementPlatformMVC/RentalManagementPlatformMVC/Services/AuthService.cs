using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentalManagementPlatformMVC.Services
{
	public class AuthService : IAuthService
	{
		private readonly RentalManagementPlatformSqlContext _db;
		private readonly PasswordHasher<Models.User> _hasher = new();

		public AuthService(RentalManagementPlatformSqlContext db) => _db = db;

		public async Task<bool> SignInAsync(HttpContext http, string usernameOrEmail, string password, bool rememberMe)
		{
			var user = await _db.Users
				.FirstOrDefaultAsync(u => u.Username == usernameOrEmail || u.Email == usernameOrEmail);
			if (user == null) return false;

			var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);
			if (result == PasswordVerificationResult.Failed) return false;

			// 取角色代碼（之後可用 [Authorize(Roles="ADMIN")]）
			var roleCodes = await (from ur in _db.UserRoles
								   join r in _db.Roles on ur.RoleId equals r.RoleId
								   where ur.UserId == user.UserId
								   select r.RoleCode).ToListAsync();

			var claims = new List<Claim>
		{
			new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
			new(ClaimTypes.Name, user.Username),
			new(ClaimTypes.Email, user.Email ?? "")
		};
			claims.AddRange(roleCodes.Select(rc => new Claim(ClaimTypes.Role, rc)));

			var id = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
			var principal = new ClaimsPrincipal(id);
			var props = new AuthenticationProperties
			{
				IsPersistent = rememberMe,
				ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
			};
			await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal, props);
			return true;
		}

		public Task SignOutAsync(HttpContext http)
			=> http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);

		public async Task<int> RegisterAsync(string username, string email, string name, string password)
		{
			if (await _db.Users.AnyAsync(u => u.Username == username || u.Email == email))
				throw new InvalidOperationException("Username or email already exists.");

			var entity = new Models.User
			{
				Username = username.Trim(),
				Email = email.Trim(),
				Name = name.Trim(),
				PasswordHash = _hasher.HashPassword(null!, password),
				Gender = "",
				Address = "",
				Isverified = false,
				BirthDate = DateTime.UtcNow.Date,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};
			_db.Users.Add(entity);
			await _db.SaveChangesAsync();
			return entity.UserId;
		}

		// ===== 忘記密碼（簡易可靠做法）=====
		// 建一張 Token 表（下段 SQL），這裡只負責產生/驗證
		public async Task<string> CreatePasswordResetAsync(string email)
		{
			var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
			if (user == null) return ""; // 不洩漏帳號存在與否

			var raw = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
			var hash = SHA256.HashData(Encoding.UTF8.GetBytes(raw));
			var token = Convert.ToHexString(hash); // 存雜湊，連結帶 raw

			_db.PasswordResetTokens.Add(new PasswordResetToken
			{
				UserId = user.UserId,
				TokenHash = token,
				ExpiresAt = DateTime.UtcNow.AddHours(1),
				CreatedAt = DateTime.UtcNow
			});
			await _db.SaveChangesAsync();

			// 你會寄出這個 URL（raw 在 querystring）
			var url = $"/Auth/ResetPassword?uid={user.UserId}&token={Uri.EscapeDataString(raw)}";
			return url;
		}

		public async Task<bool> ResetPasswordAsync(int userId, string rawToken, string newPassword)
		{
			var hash = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(rawToken)));
			var rec = await _db.PasswordResetTokens
				.Where(t => t.UserId == userId && t.UsedAt == null && t.ExpiresAt > DateTime.UtcNow)
				.OrderByDescending(t => t.CreatedAt)
				.FirstOrDefaultAsync();

			if (rec == null || !string.Equals(rec.TokenHash, hash, StringComparison.OrdinalIgnoreCase))
				return false;

			var user = await _db.Users.FirstOrDefaultAsync(u => u.UserId == userId);
			if (user == null) return false;

			user.PasswordHash = _hasher.HashPassword(user, newPassword);
			user.UpdatedAt = DateTime.UtcNow;
			rec.UsedAt = DateTime.UtcNow;

			await _db.SaveChangesAsync();
			return true;
		}
	}
}
