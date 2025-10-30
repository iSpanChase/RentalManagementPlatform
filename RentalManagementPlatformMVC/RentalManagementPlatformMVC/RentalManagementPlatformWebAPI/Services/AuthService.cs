using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;

namespace RentalManagementPlatformWebAPI.Services
{
	public sealed class PendingOperatorException : Exception
	{
		public PendingOperatorException(string message) : base(message) { }
	}
	public class AuthService : IAuthService
	{
		private readonly IUserRepository _users;
		private readonly IRoleRepository _roles;
		private readonly IPermissionRepository _perms;
		private readonly IRefreshTokenRepository _refreshRepo;
		private readonly IJwtTokenService _jwt;
		private readonly IGoogleTokenVerifier _google;
		private readonly RentalManagementPlatformSqlContext _db;
		private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
		private readonly IConfiguration _cfg;

		public AuthService(
			IUserRepository users,
			IRoleRepository roles,
			IPermissionRepository perms,
			IRefreshTokenRepository refreshRepo,
			IJwtTokenService jwt,
			IGoogleTokenVerifier google,
			RentalManagementPlatformSqlContext db,
			Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher,
			IConfiguration cfg)
		{
			_users = users; _roles = roles; _perms = perms; _refreshRepo = refreshRepo;
			_jwt = jwt; _google = google; _db = db; _hasher = hasher; _cfg = cfg;
		}

		public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
		{
			var user = await _users.GetByEmailAsync(dto.Email) ?? throw new UnauthorizedAccessException("帳號或密碼錯誤");
			if (string.IsNullOrEmpty(user.PasswordHash))
				throw new UnauthorizedAccessException("此帳號使用第三方登入，請改用 Google 登入");

			var vr = _hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);
			if (vr == PasswordVerificationResult.Failed)
				throw new UnauthorizedAccessException("帳號或密碼錯誤");
			var roleCodes = await _roles.GetCodesByUserIdAsync(user.UserId) ?? new List<string>();
			var permCodes = await _perms.GetCodesByUserIdAsync(user.UserId) ?? new List<string>();
			if (user.IsOperatorPending == true)              // ★ 新增：Operator 待審核
			{
				if (roleCodes.Any(rc => string.Equals(rc, "OPERATOR", StringComparison.OrdinalIgnoreCase)))
				{
					// 已經有 OPERATOR 角色了 → 自動清除待審核旗標
					user.IsOperatorPending = false;
					await _users.SaveChangesAsync();
				}
				else
				{
					throw new PendingOperatorException("此帳號的系統管理員身分尚未審核通過");
				}
			}

			user.LastLoginAt = DateTime.UtcNow;
			await _users.SaveChangesAsync();

			var pair = _jwt.Create(user.UserId, user.Email ?? "", user.Name ?? user.Username ?? user.Email ?? "",roleCodes, permCodes);

			var days = int.TryParse(_cfg["Jwt:RefreshTokenDays"], out var d) ? d : 7;
			await _refreshRepo.AddAsync(new RefreshToken
			{
				UserId = user.UserId,
				Token = pair.RefreshToken,
				ExpiresAt = DateTime.UtcNow.AddDays(days),
				CreatedAt = DateTime.UtcNow,
				Revoked = false
			});

			return new LoginResponseDto
			{
				AccessToken = pair.AccessToken,
				ExpiresAt = pair.ExpiresAt,
				RefreshToken = pair.RefreshToken,
				Profile = MapProfile(user),
				Roles = roleCodes,
				Permissions = permCodes
			};
		}

		public async Task<LoginResponseDto> GoogleLoginAsync(string idToken)
		{
			var aud = _cfg["Jwt:Audience"]; // 可傳 null 使用自動驗證
			var gp = await _google.VerifyAsync(idToken, aud) ?? throw new UnauthorizedAccessException("Google token 驗證失敗");

			var user = await _users.GetByProviderAsync("Google", gp.Sub);
			if (user is null)
			{
				// 若 email 已存在本地帳號，視需求：可阻擋或合併
				user = await _users.GetByEmailAsync(gp.Email);
				if (user is null)
				{
					user = new User
					{
						Email = gp.Email,
						Name = gp.Name ?? gp.Email,
						Username = MakeUsernameFromEmail(gp.Email),
						Provider = "Google",
						ProviderSubject = gp.Sub,
						ProfileImageurl = gp.Picture ?? "",
						LastLoginAt = DateTime.UtcNow,
						Isverified = true
					};
					await _users.AddAsync(user);

					// 預設給 TENANT 角色（若存在）
					var tenant = await _roles.GetByCodeAsync("TENANT");
					if (tenant != null) await _roles.AssignUserAsync(tenant.RoleId, user.UserId);
				}
				else
				{
					// 本地已有相同 email 的帳號 → 綁定 Provider
					user.Provider = "Google";
					user.ProviderSubject = gp.Sub;
					user.LastLoginAt = DateTime.UtcNow;
					await _users.SaveChangesAsync();
				}
			}
			else
			{
				user.LastLoginAt = DateTime.UtcNow;
				await _users.SaveChangesAsync();
			}

			var (roles, perms) = await GetRoleAndPermCodesAsync(user.UserId);
			var pair = _jwt.Create(user.UserId, user.Email, user.Name ?? "", roles, perms);

			var days = int.TryParse(_cfg["Jwt:RefreshTokenDays"], out var d) ? d : 7;
			await _refreshRepo.AddAsync(new RefreshToken
			{
				UserId = user.UserId,
				Token = pair.RefreshToken,
				ExpiresAt = DateTime.UtcNow.AddDays(days),
				CreatedAt = DateTime.UtcNow,
				Revoked = false
			});

			return new LoginResponseDto
			{
				AccessToken = pair.AccessToken,
				ExpiresAt = pair.ExpiresAt,
				RefreshToken = pair.RefreshToken,
				Profile = MapProfile(user),
				Roles = roles,
				Permissions = perms
			};
		}

		public async Task<LoginResponseDto> RefreshAsync(string refreshToken)
		{
			var rt = await _refreshRepo.GetAsync(refreshToken) ?? throw new UnauthorizedAccessException("Refresh token 不存在");
			if (rt.Revoked || rt.ExpiresAt <= DateTime.UtcNow) throw new UnauthorizedAccessException("Refresh token 已失效");

			var user = await _users.GetByIdAsync(rt.UserId) ?? throw new UnauthorizedAccessException("使用者不存在");
			var (roles, perms) = await GetRoleAndPermCodesAsync(user.UserId);
			var pair = _jwt.Create(user.UserId, user.Email, user.Name ?? "", roles, perms);

			// 可選：將舊 refresh 設為 revoked
			rt.Revoked = true;
			await _refreshRepo.AddAsync(new RefreshToken
			{
				UserId = user.UserId,
				Token = pair.RefreshToken,
				ExpiresAt = DateTime.UtcNow.AddDays(int.TryParse(_cfg["Jwt:RefreshTokenDays"], out var d) ? d : 7),
				CreatedAt = DateTime.UtcNow,
				Revoked = false
			});
			await _refreshRepo.SaveChangesAsync();

			return new LoginResponseDto
			{
				AccessToken = pair.AccessToken,
				ExpiresAt = pair.ExpiresAt,
				RefreshToken = pair.RefreshToken,
				Profile = MapProfile(user),
				Roles = roles,
				Permissions = perms
			};
		}

		public async Task RevokeAllAsync(int userId)
		{
			var list = await _refreshRepo.GetActiveByUserAsync(userId, DateTime.UtcNow);
			foreach (var t in list) t.Revoked = true;
			await _refreshRepo.SaveChangesAsync();
		}

		private async Task<(List<string> roles, List<string> perms)> GetRoleAndPermCodesAsync(int userId)
		{
			// 角色代碼
			var roleCodes = await _db.UserRoles
				.Where(ur => ur.UserId == userId)
				.Select(ur => ur.Role.RoleCode)
				.Distinct()
				.ToListAsync();

			// 權限代碼
			var permCodes = await _db.UserRoles
				.Where(ur => ur.UserId == userId)
				.SelectMany(ur => ur.Role.RolePermissions.Select(rp => rp.Permission.PermCode))
				.Distinct()
				.ToListAsync();

			return (roleCodes, permCodes);
		}

		private static UserProfileDto MapProfile(User u) => new()
		{
			UserId = u.UserId,
			Email = u.Email,
			Name = u.Name ?? "",
			Username = u.Username ?? "",
			Phone = u.Phone ?? "",
			ProfileImageUrl = u.ProfileImageurl ?? ""
		};

		private static string MakeUsernameFromEmail(string email)
		{
			var name = email.Split('@')[0];
			return $"{name}_{Guid.NewGuid().ToString("N")[..6]}";
		}
	}
}
