using Microsoft.AspNetCore.StaticFiles;
using RentalManagementPlatformWebAPI.DTOs;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories;
using System.Security.Claims;

namespace RentalManagementPlatformWebAPI.Services
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _users;
		private readonly IRoleRepository _roles;
		private readonly Microsoft.AspNetCore.Identity.IPasswordHasher<User> _hasher;
		private readonly IWebHostEnvironment _env;

		public UserService(IUserRepository users, IRoleRepository roles,
			Microsoft.AspNetCore.Identity.IPasswordHasher<User> hasher, IWebHostEnvironment env)
		{
			_users = users; _roles = roles; _hasher = hasher; _env = env;
		}

		public async Task<UserProfileDto> RegisterAsync(RegistrationRequestDto dto)
		{
			var exist = await _users.GetByEmailAsync(dto.Email);
			if (exist != null) throw new InvalidOperationException("Email 已被使用");

			var user = new User
			{
				Email = dto.Email,
				Name = string.IsNullOrWhiteSpace(dto.Name) ? dto.Email : dto.Name,
				Username = string.IsNullOrWhiteSpace(dto.Username) ? dto.Email.Split('@')[0] : dto.Username,
				Phone = dto.Phone ?? "",
				Provider = "Local",
				Isverified = false,
				CreatedAt = DateTime.UtcNow
			};
			// 你的 RegistrationRequestDto 若欄位是 PasswordHash/Password 擇一使用
			user.PasswordHash = _hasher.HashPassword(user, dto.PasswordHash);

			await _users.AddAsync(user);

			var role = await _roles.GetByCodeAsync(dto.RoleCode);
			if (role != null) await _roles.AssignUserAsync(role.RoleId, user.UserId);

			return Map(user);
		}

		public async Task<UserProfileDto> GetProfileAsync(ClaimsPrincipal principal)
		{
			var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
			if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException();

			var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();
			return Map(user);
		}

		// ← 改這裡：使用 UpdateProfileDto
		public async Task<UserProfileDto> UpdateProfileAsync(ClaimsPrincipal principal, UpdateProfileDto dto)
		{
			var sub = principal.FindFirstValue(ClaimTypes.NameIdentifier)
					  ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
			if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException();

			var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

			// 依你的 UpdateProfileDto 欄位更新
			user.Name = dto.Name;
			user.Phone = dto.Phone;

			await _users.SaveChangesAsync();
			return Map(user);
		}

		public async Task<string> UploadAvatarAsync(ClaimsPrincipal principal, IFormFile file)
		{
			if (file == null || file.Length == 0) throw new InvalidOperationException("檔案為空");
			var sub = principal.FindFirstValue(System.Security.Claims.ClaimTypes.NameIdentifier)
					  ?? principal.FindFirst(System.IdentityModel.Tokens.Jwt.JwtRegisteredClaimNames.Sub)?.Value;
			if (!int.TryParse(sub, out var userId)) throw new UnauthorizedAccessException();

			var user = await _users.GetByIdAsync(userId) ?? throw new UnauthorizedAccessException();

			var ext = Path.GetExtension(file.FileName);
			var fname = $"avatar_{userId}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}{ext}";
			var folder = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "avatars");
			Directory.CreateDirectory(folder);
			var full = Path.Combine(folder, fname);

			using (var fs = File.Create(full)) { await file.CopyToAsync(fs); }

			var provider = new FileExtensionContentTypeProvider();
			if (!provider.TryGetContentType(full, out var contentType)) contentType = "application/octet-stream";

			user.ProfileImageurl = $"/avatars/{fname}";
			await _users.SaveChangesAsync();
			return user.ProfileImageurl ?? "";
		}

		private static UserProfileDto Map(User u) => new()
		{
			UserId = u.UserId,
			Email = u.Email,
			Name = u.Name ?? "",
			Username = u.Username ?? "",
			Phone = u.Phone ?? "",
			ProfileImageurl = u.ProfileImageurl ?? ""
		};
	}
}
