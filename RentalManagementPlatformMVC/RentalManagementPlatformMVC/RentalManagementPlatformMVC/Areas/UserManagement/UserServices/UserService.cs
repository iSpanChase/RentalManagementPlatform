using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.UserManagement.Module;
using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Models;
using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserServices
{
	public class UserService : IUserService
	{
		private readonly IUserRepository _userRepo;
		private readonly IRepository<UserRole> _userRoleRepo;
		private readonly IRepository<Role> _roleRepo;
		private readonly IUnitOfWork _uow;

		public UserService(
			IUserRepository userRepo,
			IRepository<UserRole> userRoleRepo,
			IRepository<Role> roleRepo,
			IUnitOfWork uow)
		{
			_userRepo = userRepo;
			_userRoleRepo = userRoleRepo;
			_roleRepo = roleRepo;
			_uow = uow;
		}

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

		public async Task<OpResult> DeleteAsync(int userId, CancellationToken ct = default)
		{
			// 1) 先查是否存在
			var user = await _userRepo.GetByIdAsync(userId);
			if (user == null)
				return OpResult.Fail("NotFound", "找不到該帳號。");

			// 2) 檢查是否仍有角色關聯（USER_ROLES 多對多）
			//    透過既有的通用 repo 查關聯表 UserRole（用 predicate 篩選）
			var roleLinks = await _userRoleRepo.ListAsync(ur => ur.UserId == userId);
			if (roleLinks.Count > 0)
				return OpResult.Fail("HasRoles", "無法刪除該帳號，請先刪除其角色權限。");

			// 3) 安全刪除 + 提交
			_userRepo.Remove(user);
			await _uow.SaveChangesAsync(ct);

			return OpResult.Ok();
		}


		public async Task<AssignUserRolesVm> GetAssignRolesAsync(int userId)
		{
			var u = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");

			var assignedIds = (await _userRoleRepo.ListAsync(ur => ur.UserId == userId))
							  .Select(ur => ur.RoleId).ToHashSet();

			var allRoles = await _roleRepo.ListAsync(); // 以名稱排序可在 View 做
			return new AssignUserRolesVm
			{
				UserId = u.UserId,
				Username = u.Username,
				Email = u.Email,
				Name = u.Name,
				Roles = allRoles.OrderBy(r => r.RoleName).Select(r => new RoleCheckItem
				{
					RoleId = r.RoleId,
					RoleCode = r.RoleCode,
					RoleName = r.RoleName,
					Checked = assignedIds.Contains(r.RoleId)
				}).ToList(),
				SelectedRoleIds = assignedIds.ToArray()
			};
		}

		public async Task AssignRolesAsync(int userId, int[] roleIds)
		{
			// 確認使用者存在
			_ = await _userRepo.GetByIdAsync(userId) ?? throw new KeyNotFoundException("User not found");

			// 目前角色
			var current = await _userRoleRepo.ListAsync(ur => ur.UserId == userId);
			var currentIds = current.Select(c => c.RoleId).ToHashSet();

			// 目標角色（去重）
			var want = new HashSet<int>((roleIds ?? Array.Empty<int>()).Distinct());

			// 新增
			foreach (var rid in want.Except(currentIds))
				await _userRoleRepo.AddAsync(new UserRole { UserId = userId, RoleId = rid });

			// 移除
			foreach (var ur in current.Where(c => !want.Contains(c.RoleId)))
				_userRoleRepo.Remove(ur);

			await _uow.SaveChangesAsync(); // UoW 統一提交（你現成的 SaveChanges）:contentReference[oaicite:4]{index=4}
		}
	}
}
