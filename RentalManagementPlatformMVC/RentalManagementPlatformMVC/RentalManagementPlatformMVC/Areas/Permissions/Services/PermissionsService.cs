using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs;
using RentalManagementPlatformMVC.Areas.Permissions.PermissionsRepositories;
using RentalManagementPlatformMVC.Areas.Permissions.ViewModels;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.CommonRepos;
using RentalManagementPlatformMVC.Models;
using PermissionEntity = RentalManagementPlatformMVC.Models.Permission;

namespace RentalManagementPlatformMVC.Areas.Permissions.Services
{
	public class PermissionsService : IPermissionsService
	{
		private readonly IPermissionsRepository _repo;
		private readonly IUnitOfWork _uow;
		private readonly RentalManagementPlatformSqlContext _db;

		public PermissionsService(IPermissionsRepository repo, IUnitOfWork uow, RentalManagementPlatformSqlContext db)
		{
			_repo = repo;
			_uow = uow;
			_db = db;
		}

		public async Task<PagedResult<PermissionListItemDto>> QueryAsync(PermissionQueryInput input)
		{
			var q = _repo.Query();

			if (!string.IsNullOrWhiteSpace(input.Keyword))
			{
				var kw = input.Keyword.Trim();
				q = q.Where(p =>
					p.PermCode.Contains(kw) ||
					p.PermName.Contains(kw) ||
					p.Module.Contains(kw) ||
					p.Action.Contains(kw) ||
					(p.Description != null && p.Description.Contains(kw)));
			}
			if (!string.IsNullOrWhiteSpace(input.Module))
			{
				var m = input.Module.Trim();
				q = q.Where(p => p.Module == m);
			}
			if (!string.IsNullOrWhiteSpace(input.Action))
			{
				var a = input.Action.Trim();
				q = q.Where(p => p.Action == a);
			}

			q = input.SortBy?.ToLower() switch
			{
				"code" => input.Desc ? q.OrderByDescending(p => p.PermCode) : q.OrderBy(p => p.PermCode),
				"name" => input.Desc ? q.OrderByDescending(p => p.PermName) : q.OrderBy(p => p.PermName),
				"module" => input.Desc ? q.OrderByDescending(p => p.Module) : q.OrderBy(p => p.Module),
				"action" => input.Desc ? q.OrderByDescending(p => p.Action) : q.OrderBy(p => p.Action),
				"created" => input.Desc ? q.OrderByDescending(p => p.CreatedAt) : q.OrderBy(p => p.CreatedAt),
				_ => q.OrderBy(p => p.Module).ThenBy(p => p.Action).ThenBy(p => p.PermCode)
			};

			var total = await q.CountAsync();
			var items = await q.Skip((input.Page - 1) * input.PageSize)
				.Take(input.PageSize)
				.Select(p => new PermissionListItemDto(
					p.PermissionId, p.PermCode, p.PermName, p.Module, p.Action, p.Description, p.CreatedAt, p.UpdatedAt))
				.ToListAsync();

			return new PagedResult<PermissionListItemDto> { Total = total, Items = items };
		}

		public async Task<int> CreateAsync(CreatePermissionDto dto)
		{
			dto.PermCode = dto.PermCode.Trim();
			dto.PermName = dto.PermName.Trim();
			dto.Module = dto.Module.Trim();
			dto.Action = dto.Action.Trim();
			if (await _repo.ExistsByCodeAsync(dto.PermCode))
				throw new InvalidOperationException("perm_code 已存在");
			if (await _repo.ExistsByModuleActionAsync(dto.Module, dto.Action))
				throw new InvalidOperationException("(module, action) 組合已存在");

			var entity = new PermissionEntity
			{
				PermCode = dto.PermCode,
				PermName = dto.PermName,
				Module = dto.Module,
				Action = dto.Action,
				Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim(),
				CreatedAt = DateTime.UtcNow,
				UpdatedAt = DateTime.UtcNow
			};
			await _repo.AddAsync(entity);
			await _uow.SaveChangesAsync();
			return entity.PermissionId;
		}

		public async Task UpdateAsync(int id, UpdatePermissionDto dto)
		{
			var entity = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Permission not found");
			var code = dto.PermCode.Trim();
			var name = dto.PermName.Trim();
			var module = dto.Module.Trim();
			var action = dto.Action.Trim();

			if (await _repo.ExistsByCodeAsync(code, excludeId: id))
				throw new InvalidOperationException("perm_code 已存在");
			if (await _repo.ExistsByModuleActionAsync(module, action, excludeId: id))
				throw new InvalidOperationException("(module, action) 組合已存在");

			entity.PermCode = code;
			entity.PermName = name;
			entity.Module = module;
			entity.Action = action;
			entity.Description = string.IsNullOrWhiteSpace(dto.Description) ? null : dto.Description!.Trim();
			entity.UpdatedAt = DateTime.UtcNow;

			_repo.Update(entity);
			await _uow.SaveChangesAsync();
		}

		public async Task DeleteAsync(int id)
		{
			var entity = await _repo.GetByIdAsync(id) ?? throw new KeyNotFoundException("Permission not found");
			_repo.Remove(entity);
			await _uow.SaveChangesAsync();
		}

		public async Task<PermissionDetailDto?> GetDetailAsync(int id)
		{
			return await _repo.Query()
				.Where(p => p.PermissionId == id)
				.Select(p => new PermissionDetailDto
				{
					PermissionId = p.PermissionId,
					PermCode = p.PermCode,
					PermName = p.PermName,
					Module = p.Module,
					Action = p.Action,
					Description = p.Description,
					CreatedAt = p.CreatedAt,
					UpdatedAt = p.UpdatedAt
				})
				.FirstOrDefaultAsync();
		}

		public async Task<PermissionUsedByRolesVm> GetUsedByRolesVmAsync(int permissionId)
		{
			var perm = await _repo.Query()
				.Where(p => p.PermissionId == permissionId)
				.Select(p => new { p.PermissionId, p.PermCode, p.PermName, p.Module, p.Action })
				.FirstOrDefaultAsync();
			if (perm is null) throw new KeyNotFoundException("Permission not found");

			// 只做唯讀查詢，不做指派（避免和 RolesService 重疊）
			var roles = await _db.RolePermissions
				.Where(rp => rp.PermissionId == permissionId)
				.Select(rp => new {
					rp.Role.RoleId,
					rp.Role.RoleCode,
					rp.Role.RoleName
				})
				.OrderBy(r => r.RoleName)
				.ToListAsync();

			return new PermissionUsedByRolesVm
			{
				PermissionId = perm.PermissionId,
				PermCode = perm.PermCode,
				PermName = perm.PermName,
				Module = perm.Module,
				Action = perm.Action,
				Roles = roles.Select(r => new SimpleRoleVm(r.RoleId, r.RoleCode, r.RoleName)).ToList()
			};
		}
	}
}
