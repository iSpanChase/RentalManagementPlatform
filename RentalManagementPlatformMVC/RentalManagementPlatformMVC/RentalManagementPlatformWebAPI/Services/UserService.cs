using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _users;
		private readonly IRoleRepository _roles;
		private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
		private readonly IWebHostEnvironment _env;

		public UserService(
			IUserRepository users,
			IRoleRepository roles,
			Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher,
			IWebHostEnvironment env)
		{
			_users = users;
			_roles = roles;
			_hasher = hasher;
			_env = env;
		}

		private static int GetUserIdFromClaims(ClaimsPrincipal principal)
		{
			var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
			if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException();
			return userId;
		}

		public async Task<UserProfileDto> RegisterAsync(RegistrationRequestDto dto)
		{
			var exist = await _users.GetByEmailAsync(dto.Email);
			if (exist != null) throw new InvalidOperationException("Email 已被使用");
			var existUsername = await _users.Query().AnyAsync(u => u.Username == dto.Username);
			if (existUsername) throw new InvalidOperationException("此帳號已被使用");

			var user = new User
			{
				Email = dto.Email,
				Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email : dto.Name,
				Username = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email.Split('@')[0] : dto.Username,
				Gender = dto.Gender,
				BirthDate = dto.BirthDate,
				Address = dto.Address,
				Phone = dto.Phone ?? "",
				ProfileImageurl = dto.ProfileImageUrl,   // 實體屬性是小寫 u
				Provider = "Local",
				Isverified = false,
				CreatedAt = DateTime.UtcNow
			};

			user.PasswordHash = _hasher.HashPassword(user, dto.PasswordHash);

			await _users.AddAsync(user);

			var role = await _roles.GetByCodeAsync(dto.RoleCode);
			if (role != null) await _roles.AssignUserAsync(role.RoleId, user.UserId);

			return Map(user);
		}

		public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal)
		{
			var userId = GetUserIdFromClaims(principal);
			var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();
			return Map(user);
		}

		public async Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto)
		{
			var userId = GetUserIdFromClaims(principal);

			// 以 repository 取回目前使用者
			var entity = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

			// === 逐欄更新（未送的欄位保持原值；此處依照前端送入的 DTO 全覆蓋） ===
			entity.Name = dto.Name;
			entity.Gender = dto.Gender;
			entity.BirthDate = dto.BirthDate;           // 前端送 yyyy-MM-dd，binder 已轉 DateTime
			entity.Address = dto.Address;

			entity.Phone = dto.Phone;                   // 可 null
			entity.Point = dto.Point;                   // 可 null
			entity.ProfileImageurl = dto.ProfileImageUrl; // 實體屬性命名為 ProfileImageurl（小寫 u）

			entity.UpdatedAt = DateTime.UtcNow;

			await _users.SaveChangesAsync();

			// 統一回傳最新 Profile（建議 Controller 直接 Ok(...) 回這份）
			return Map(entity);
		}

		public async Task<string> UploadAvatarAsync(ClaimsPrincipal principal, IFormFile file)
		{
			if (file == null || file.Length == 0) throw new InvalidOperationException("檔案為空");

			var userId = GetUserIdFromClaims(principal);
			var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

			var ext = Path.GetExtension(file.FileName);
			var fname = $"avatar_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
			var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
			Directory.CreateDirectory(folder);
			var full = Path.Combine(folder, fname);

			using (var fs = File.Create(full))
			{
				await file.CopyToAsync(fs);
			}

			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(full, out var contentType))
				contentType = "application/octet-stream";

			user.ProfileImageurl = $"/avatars/{fname}";
			await _users.SaveChangesAsync();
			return user.ProfileImageurl ?? "";
		}

		// === 實體 → 前端用 DTO 的映射，欄位與前端完全對齊 ===
		private static UserProfileDto Map(User u) => new()
		{
			UserId = u.UserId,
			Username = u.Username ?? "",
			Email = u.Email ?? "",
			Name = u.Name ?? "",
			Gender = u.Gender ?? "",
			BirthDate = u.BirthDate,
			Phone = u.Phone ?? "",
			Address = u.Address ?? "",
			Point = u.Point,
			ProfileImageUrl = u.ProfileImageurl, // 注意命名差異
			IsVerified = u.Isverified
		};
	}
}
