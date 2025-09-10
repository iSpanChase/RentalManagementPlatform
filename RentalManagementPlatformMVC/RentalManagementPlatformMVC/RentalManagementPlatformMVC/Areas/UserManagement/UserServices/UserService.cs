using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserServices
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepo;
		private readonly IUnitOfWork _uow;

		public UserService(IUserRepository userRepo, IUnitOfWork uow)
			=> (_userRepo, _uow) = (userRepo, uow);

		public async Task<(IReadOnlyList<UserListItemDto> Items, int Total)> ListAsync(string? keyword, int page, int pageSize)
		{
			page = page <= 0 ? 1 : page;
			pageSize = pageSize <= 0 ? 10 : pageSize;

			var q = _userRepo.Query();

			if (!string.IsNullOrWhiteSpace(keyword))
				q = q.Where(x => x.Username.Contains(keyword) || x.Email.Contains(keyword) || x.Name.Contains(keyword));

			var total = await q.CountAsync();
			var items = await q.OrderByDescending(x => x.CreatedAt)
							   .Skip((page - 1) * pageSize)
							   .Take(pageSize)
							   .Select(x => new UserListItemDto(x.UserId, x.Username, x.Email, x.Name, (DateTime)x.CreatedAt))
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
				CreatedAt = (DateTime)x.CreatedAt,
				UpdatedAt = (DateTime)x.UpdatedAt
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
				BirthDate = (DateTime)dto.BirthDate,
				Phone = dto.Phone,
				Address = dto.Address,
				Point = dto.Point,
				Isverified = (bool)dto.Isverified,
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
			user.BirthDate = (DateTime)dto.BirthDate;
			user.Phone = dto.Phone;
			user.Address = dto.Address;
			user.Point = dto.Point;
			user.Isverified = (bool)dto.Isverified;
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
