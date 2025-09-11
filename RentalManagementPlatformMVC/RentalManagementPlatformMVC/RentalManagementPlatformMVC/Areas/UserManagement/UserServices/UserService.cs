using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories;
using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserServices
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepo;
		private readonly IUnitOfWork _uow;

		public UserService(IUserRepository userRepo, IUnitOfWork uow)
			=> (_userRepo, _uow) = (userRepo, uow);

		public async Task<(IReadOnlyList<UserListItemDto> Items, int Total)> ListAsync(UserFilterVm f)
		{
			var page = f.Page <= 0 ? 1 : f.Page;
			var pageSize = f.PageSize <= 0 ? 10 : f.PageSize;

			var q = _userRepo.Query(); // 建議 Query() 內部就用 AsNoTracking()

			// 關鍵字（帳號/Email/姓名）
			if (!string.IsNullOrWhiteSpace(f.Keyword))
				q = q.Where(x => x.Username.Contains(f.Keyword) ||
								 x.Email.Contains(f.Keyword) ||
								 x.Name.Contains(f.Keyword));

			// 性別
			if (!string.IsNullOrWhiteSpace(f.Gender))
				q = q.Where(x => x.Gender == f.Gender);

			// 是否驗證
			if (f.Isverified.HasValue)
				q = q.Where(x => x.Isverified == f.Isverified.Value);

			// 建立時間區間（右界 +1 天，含當日）
			if (f.CreatedFrom.HasValue)
				q = q.Where(x => x.CreatedAt >= f.CreatedFrom.Value);
			if (f.CreatedTo.HasValue)
			{
				var end = f.CreatedTo.Value.Date.AddDays(1);
				q = q.Where(x => x.CreatedAt < end);
			}

			// 排序
			var sortBy = (f.SortBy ?? "createdAt").ToLowerInvariant();
			var desc = string.Equals(f.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
			q = sortBy switch
			{
				"id" => desc ? q.OrderByDescending(x => x.UserId) : q.OrderBy(x => x.UserId),
				"username" => desc ? q.OrderByDescending(x => x.Username) : q.OrderBy(x => x.Username),
				"name" => desc ? q.OrderByDescending(x => x.Name) : q.OrderBy(x => x.Name),
				"email" => desc ? q.OrderByDescending(x => x.Email) : q.OrderBy(x => x.Email),
				_ => desc ? q.OrderByDescending(x => x.CreatedAt) : q.OrderBy(x => x.CreatedAt),
			};

			var total = await q.CountAsync();

			var items = await q.Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .Select(x => new UserListItemDto(
								   x.UserId, x.Username, x.Email, x.Name,
								   x.CreatedAt ?? DateTime.MinValue))
							   .ToListAsync();

			return (items, total);
		}

		public async Task<UserDetailDto> GetAsync(int userId)
		{
			var x = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");
			return new UserDetailDto
			{
				UserId = x.UserId,
				Username = x.Username,
				Email = x.Email,
				Name = x.Name,
				Gender = x.Gender,
				BirthDate = x.BirthDate,
				Phone = x.Phone,
				Address = x.Address,
				Point = x.Point,
				Isverified = x.Isverified,
				ProfileImageurl = x.ProfileImageurl,
				CreatedAt = x.CreatedAt,
				UpdatedAt = x.UpdatedAt
			};
		}

		public async Task<int> CreateAsync(CreateUserDto dto)
		{
			if (await _userRepo.ExistsByUsernameAsync(dto.Username))
				throw new InvalidOperationException("Username 已被使用");
			if (await _userRepo.ExistsByEmailAsync(dto.Email))
				throw new InvalidOperationException("Email 已被使用");

			var user = new UserEntity
			{
				Username = dto.Username,
				Email = dto.Email,
				Name = dto.Name,
				PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.PasswordHash),
				Gender = dto.Gender,
				BirthDate = dto.BirthDate,
				Phone = dto.Phone,
				Address = dto.Address,
				Point = dto.Point,
				Isverified = dto.Isverified,
				ProfileImageurl = dto.ProfileImageurl,
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};

			await _userRepo.AddAsync(user);
			await _uow.SaveChangesAsync();
			return user.UserId;
		}

		public async Task UpdateAsync(UpdateUserDto dto)
		{
			var user = await _userRepo.GetByIdAsync(dto.UserId) ?? throw new KeyNotFoundException("User not found");

			// 不允許在此修改 Username / Password
			user.Email = dto.Email;
			user.Name = dto.Name;
			user.Gender = dto.Gender;
			user.BirthDate = dto.BirthDate;
			user.Phone = dto.Phone;
			user.Address = dto.Address;
			user.Point = dto.Point;
			user.Isverified = dto.Isverified;
			user.ProfileImageurl = dto.ProfileImageurl;
			user.UpdatedAt = DateTime.UtcNow;

			_userRepo.Update(user);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteAsync(int userId)
		{
			var user = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");
			_userRepo.Remove(user);
			await _uow.SaveChangesAsync();
		}
	}
}
