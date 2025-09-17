using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Auth.ViewModels;
using RentalManagementPlatformMVC.Models;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace RentalManagementPlatformMVC.Areas.Auth.Services
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

			// 1) 取角色代碼（可用 [Authorize(Roles="ADMIN")]）
			var roleCodes = await (from ur in _db.UserRoles
								   join r in _db.Roles on ur.RoleId equals r.RoleId
								   where ur.UserId == user.UserId
								   select r.RoleCode).ToListAsync();
			// 2) 取該使用者透過角色得到的 perm_code
			var permCodes = await (from ur in _db.UserRoles
								   join rp in _db.RolePermissions on ur.RoleId equals rp.RoleId
								   join p in _db.Permissions on rp.PermissionId equals p.PermissionId
								   where ur.UserId == user.UserId
								   select p.PermCode)
								  .Distinct()
								  .ToListAsync();

			// 3) 建立 Claims：基本 + 角色 + 權限
			var claims = new List<Claim>
			{
				new(ClaimTypes.NameIdentifier, user.UserId.ToString()),
				new(ClaimTypes.Name, user.Username),
				new(ClaimTypes.Email, user.Email ?? "")
			};
			claims.AddRange(roleCodes.Select(rc => new Claim(ClaimTypes.Role, rc)));
			claims.AddRange(permCodes.Select(pc => new Claim("permission", pc))); // ★ 核心：把 perm_code 灌進來

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

		public async Task<int> RegisterAsync(RegisterVm vm)
		{
			if (await _db.Users.AnyAsync(u => u.Username == vm.Username || u.Email == vm.Email))
				throw new InvalidOperationException("Username or email already exists.");

			var entity = new Models.User
			{
				Name = vm.Name.Trim(),
				Email = vm.Email.Trim(),
				Username = vm.Username.Trim(),				
				PasswordHash = _hasher.HashPassword(null!, vm.Password),
				Gender = vm.Gender.Trim(),                    
				BirthDate = vm.BirthDate,                     
				Phone = vm.Phone.Trim(),                      
				Address = vm.Address.Trim(),                  
				ProfileImageurl = string.IsNullOrWhiteSpace(vm.ProfileImageurl)
							? null : vm.ProfileImageurl!.Trim(), // ← 可為 null
				Isverified = false,
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
