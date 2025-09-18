using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.Areas.Roles.RolesRepositories;
using RentalManagementPlatformMVC.Areas.Roles.ViewModels;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Models;
using RolesEntity = RentalManagementPlatformMVC.Models.Role;

namespace RentalManagementPlatformMVC.Areas.Roles.RolesServices
{
	public class RolesService : IRolesService
	{
		private readonly IRolesRepository _repo;
		private readonly IUnitOfWork _uow;
		private readonly RentalManagementPlatformSqlContext _db;
		public RolesService(IRolesRepository repo, IUnitOfWork uow, RentalManagementPlatformSqlContext db)
		{
			_repo = repo;
			_uow = uow;
			_db = db;
		}

		public async Task<PagedResult<RolesListItemDto>> QueryAsync(RoleQueryInput input)
		{
			var q = _repo.Query()
				.Select(r => new {
					r.RoleId,
					r.RoleCode,
					r.RoleName,
					r.Description,
					UserCount = r.UserRoles.Count(),
					PermCount = r.RolePermissions.Count()
				});

			if (!string.IsNullOrWhiteSpace(input.RoleName))
				q = q.Where(x => x.RoleName.Contains(input.RoleName));

			if (!string.IsNullOrWhiteSpace(input.RoleCode))
				q = q.Where(x => x.RoleCode.Contains(input.RoleCode));

			if (!string.IsNullOrWhiteSpace(input.Description))
				q = q.Where(x => x.Description != null && x.Description.Contains(input.Description));

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var kw = input.Keyword.Trim();
				q = q.Where(x =>
					x.RoleName.Contains(kw) ||
					x.RoleCode.Contains(kw) ||
					(x.Description != null && x.Description.Contains(kw))
				);
			}

			q = input.SortBy switch
			{
				"RoleCode" => input.Desc ? q.OrderByDescending(x => x.RoleCode) : q.OrderBy(x => x.RoleCode),
				"UserCount" => input.Desc ? q.OrderByDescending(x => x.UserCount) : q.OrderBy(x => x.UserCount),
				"PermCount" => input.Desc ? q.OrderByDescending(x => x.PermCount) : q.OrderBy(x => x.PermCount),
				_ => input.Desc ? q.OrderByDescending(x => x.RoleName) : q.OrderBy(x => x.RoleName),
			};

			var total = await q.CountAsync();
			var items = await q.Skip((input.Page - 1) * input.PageSize)
							   .Take(input.PageSize)
							   .Select(x => new RolesListItemDto(x.RoleId, x.RoleCode, x.RoleName, x.UserCount, x.PermCount))
							   .ToListAsync();

			return new PagedResult<RolesListItemDto> { Total = total, Items = items };
		}

		public async Task<int> CreateAsync(CreateRolesDto dto)
		{
			var entity = new RolesEntity
			{
				RoleCode = dto.RoleCode.Trim(),
				RoleName = dto.RoleName.Trim(),
				Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};
			await _repo.AddAsync(entity);                 // 透過泛型 Repo 新增（不提交）:contentReference[oaicite:14]{index=14} :contentReference[oaicite:15]{index=15}
			await _uow.SaveChangesAsync();                // 由 UoW 統一提交 :contentReference[oaicite:16]{index=16}
			return entity.RoleId;
		}

		public async Task UpdateAsync(int roleId, UpdateRolesDto dto)
		{
			var entity = await _repo.GetByIdAsync(roleId)
						 ?? throw new KeyNotFoundException("Role not found");
			entity.RoleCode = dto.RoleCode.Trim();
			entity.RoleName = dto.RoleName.Trim();
			entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description.Trim();
			entity.UpdatedAt = DateTime.UtcNow;

			_repo.Update(entity);                         // 標記更新（不提交）:contentReference[oaicite:17]{index=17}
			await _uow.SaveChangesAsync();                // 提交變更 :contentReference[oaicite:18]{index=18}
		}

		public async Task DeleteAsync(int roleId)
		{
			var entity = await _repo.GetByIdAsync(roleId)
						 ?? throw new KeyNotFoundException("Role not found");
			_repo.Remove(entity);                         // 標記刪除（不提交）:contentReference[oaicite:19]{index=19}
			await _uow.SaveChangesAsync();                // 提交變更 :contentReference[oaicite:20]{index=20}
		}

		public async Task<RolesDetailDto?> GetDetailAsync(int roleId)
		{
			// 直接用 Query() 投影到 DetailDto（NoTracking）:contentReference[oaicite:21]{index=21}
			return await _repo.Query()
				.Where(r => r.RoleId == roleId)
				.Select(r => new RolesDetailDto
				{
					RoleCode = r.RoleCode,
					RoleName = r.RoleName,
					Description = r.Description
				})
				.FirstOrDefaultAsync();
		}

		// ---------- 指派使用者 ----------
		public async Task<AssignUsersVm> GetAssignUsersVmAsync(int roleId)
		{
			var role = await _repo.Query()
				.Where(r => r.RoleId == roleId)
				.Select(r => new { r.RoleId, r.RoleName })
				.FirstOrDefaultAsync();
			if (role is null) throw new KeyNotFoundException("Role not found");

			var selectedIds = await _db.UserRoles
				.Where(ur => ur.RoleId == roleId)
				.Select(ur => ur.UserId)
				.ToListAsync();

			var allUsers = await _db.Users
				.OrderBy(u => u.Name)
				.Select(u => new SelectListItem
				{
					Value = u.UserId.ToString(),
					Text = (u.Name ?? u.Username) + (string.IsNullOrEmpty(u.Email) ? "" : $" ({u.Email})"),
					Selected = selectedIds.Contains(u.UserId)
				}).ToListAsync();

			return new AssignUsersVm
			{
				RoleId = role.RoleId,
				RoleName = role.RoleName,
				SelectedUserIds = selectedIds,
				AllUsers = allUsers
			};
		}

		public async Task<List<int>> GetUserIdsInRoleAsync(int roleId)
		{
			return await _repo.GetUserIdsInRoleAsync(roleId);
		}

		public async Task SaveAssignUsersAsync(AssignUsersVm vm)
		{
			var role = await _repo.GetByIdAsync(vm.RoleId) ?? throw new KeyNotFoundException("Role not found");

			var target = (vm.SelectedUserIds ?? new List<int>()).Distinct().ToHashSet();
			var current = await _db.UserRoles
				.Where(ur => ur.RoleId == vm.RoleId)
				.Select(ur => ur.UserId)
				.ToListAsync();
			var curSet = current.ToHashSet();

			var toAdd = target.Except(curSet).ToList();
			var toRemove = curSet.Except(target).ToList();

			if (toRemove.Count > 0)
			{
				var removeRows = await _db.UserRoles
					.Where(ur => ur.RoleId == vm.RoleId && toRemove.Contains(ur.UserId))
					.ToListAsync();
				_db.UserRoles.RemoveRange(removeRows);
			}

			foreach (var uid in toAdd)
			{
				_db.UserRoles.Add(new UserRole
				{
					RoleId = vm.RoleId,
					UserId = uid,
					CreatedAt = DateTime.UtcNow
				});
			}

			await _uow.SaveChangesAsync();
		}

		// ---------- 指派權限 ----------
		public async Task<AssignPermissionsVm> GetAssignPermissionsVmAsync(int roleId)
		{
			var role = await _repo.Query()
				.Where(r => r.RoleId == roleId)
				.Select(r => new { r.RoleId, r.RoleName })
				.FirstOrDefaultAsync();
			if (role is null) throw new KeyNotFoundException("Role not found");

			var selectedIds = await _db.RolePermissions
				.Where(rp => rp.RoleId == roleId)
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			var allPerms = await _db.Permissions
				.OrderBy(p => p.Module).ThenBy(p => p.Action)
				.Select(p => new SelectListItem
				{
					Value = p.PermissionId.ToString(),
					Text = $"{p.Module}:{p.Action} - {p.PermName}",
					Selected = selectedIds.Contains(p.PermissionId)
				}).ToListAsync();

			return new AssignPermissionsVm
			{
				RoleId = role.RoleId,
				RoleName = role.RoleName,
				SelectedPermissionIds = selectedIds,
				AllPermissions = allPerms
			};
		}

		public async Task<List<int>> GetPermissionIdsInRoleAsync(int roleId)
		{
			return await _repo.GetPermissionIdsInRoleAsync(roleId);
		}

		public async Task SaveAssignPermissionsAsync(AssignPermissionsVm vm)
		{
			var role = await _repo.GetByIdAsync(vm.RoleId) ?? throw new KeyNotFoundException("Role not found");

			var target = (vm.SelectedPermissionIds ?? new List<int>()).Distinct().ToHashSet();
			var current = await _db.RolePermissions
				.Where(rp => rp.RoleId == vm.RoleId)
				.Select(rp => rp.PermissionId)
				.ToListAsync();
			var curSet = current.ToHashSet();

			var toAdd = target.Except(curSet).ToList();
			var toRemove = curSet.Except(target).ToList();

			if (toRemove.Count > 0)
			{
				var removeRows = await _db.RolePermissions
					.Where(rp => rp.RoleId == vm.RoleId && toRemove.Contains(rp.PermissionId))
					.ToListAsync();
				_db.RolePermissions.RemoveRange(removeRows);
			}

			foreach (var pid in toAdd)
			{
				_db.RolePermissions.Add(new RolePermission
				{
					RoleId = vm.RoleId,
					PermissionId = pid,
					CreatedAt = DateTime.UtcNow
				});
			}

			await _uow.SaveChangesAsync();
		}

		public async Task<string> GetRoleNameAsync(int roleId)
	=> await _repo.Query().Where(r => r.RoleId == roleId)
		   .Select(r => r.RoleName)
		   .FirstOrDefaultAsync() ?? "(Unknown)";

		public async Task<PagedResult<AssignableUserListItemDto>> QueryAssignableUsersAsync(AssignUsersQueryInput input)
		{
			var sort = (input.SortBy ?? "Name").ToLowerInvariant();
			var desc = input.Desc;

			// 第一步：只查欄位 (EF 能翻譯)
			var baseQ = _db.Users
				.Select(u => new { u.UserId, u.Username, u.Name, u.Email });

			if (!string.IsNullOrEmpty(input.Keyword))
			{
				baseQ = baseQ.Where(u =>
					(u.Name != null && u.Name.Contains(input.Keyword)) ||
					(u.Username != null && u.Username.Contains(input.Keyword)) ||
					(u.Email != null && u.Email.Contains(input.Keyword)));
			}

			// 第二步：EF 排序 (用原生欄位，不用 DTO)
			IOrderedQueryable<dynamic> ordered = sort switch
			{
				"username" => desc ? baseQ.OrderByDescending(x => x.Username) : baseQ.OrderBy(x => x.Username),
				"email" => desc ? baseQ.OrderByDescending(x => x.Email) : baseQ.OrderBy(x => x.Email),
				_ => desc ? baseQ.OrderByDescending(x => x.Name) : baseQ.OrderBy(x => x.Name),
			};

			var total = await ordered.CountAsync();

			// 第三步：取出資料，再轉 DTO
			var itemsRaw = await ordered
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

			var userIds = itemsRaw.Select(x => x.UserId).ToList();
			var selectedIds = await _db.UserRoles
				.Where(ur => ur.RoleId == input.RoleId && userIds.Contains(ur.UserId))
				.Select(ur => ur.UserId)
				.ToListAsync();

			var items = itemsRaw
				.Select(x => new AssignableUserListItemDto(
					x.UserId,
					x.Username,
					string.IsNullOrEmpty(x.Name) ? x.Username : x.Name,
					x.Email ?? "",
					selectedIds.Contains(x.UserId)))
				.ToList();

			return new PagedResult<AssignableUserListItemDto> { Total = total, Items = items };
		}

		public async Task<PagedResult<AssignablePermissionListItemDto>> QueryAssignablePermissionsAsync(AssignPermissionsQueryInput input)
		{
			var sort = (input.SortBy ?? "Code").ToLowerInvariant();
			var desc = input.Desc;

			// 第一步：先查欄位
			var baseQ = _db.Permissions
				.Select(p => new { p.PermissionId, p.Module, p.Action, p.PermName, p.PermCode });

			if (!string.IsNullOrEmpty(input.Keyword))
			{
				baseQ = baseQ.Where(p =>
					(p.PermName != null && p.PermName.Contains(input.Keyword)) ||
					(p.Module != null && p.Module.Contains(input.Keyword)) ||
					(p.Action != null && p.Action.Contains(input.Keyword)) ||
					(p.PermCode != null && p.PermCode.Contains(input.Keyword)));
			}

			// 第二步：排序
			IOrderedQueryable<dynamic> ordered = sort switch
			{
				"module" => desc ? baseQ.OrderByDescending(x => x.Module) : baseQ.OrderBy(x => x.Module),
				"action" => desc ? baseQ.OrderByDescending(x => x.Action) : baseQ.OrderBy(x => x.Action),
				"name" => desc ? baseQ.OrderByDescending(x => x.PermName) : baseQ.OrderBy(x => x.PermName),
				_ => desc ? baseQ.OrderByDescending(x => x.PermCode) : baseQ.OrderBy(x => x.PermCode),
			};

			var total = await ordered.CountAsync();

			var itemsRaw = await ordered
				.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.ToListAsync();

			var permIds = itemsRaw.Select(x => x.PermissionId).ToList();
			var selectedIds = await _db.RolePermissions
				.Where(rp => rp.RoleId == input.RoleId && permIds.Contains(rp.PermissionId))
				.Select(rp => rp.PermissionId)
				.ToListAsync();

			var items = itemsRaw
				.Select(x => new AssignablePermissionListItemDto(
					x.PermissionId,
					x.Module ?? "",
					x.Action ?? "",
					x.PermName ?? "",
					x.PermCode ?? "",
					selectedIds.Contains(x.PermissionId)))
				.ToList();

			return new PagedResult<AssignablePermissionListItemDto> { Total = total, Items = items };
		}
	}
}
