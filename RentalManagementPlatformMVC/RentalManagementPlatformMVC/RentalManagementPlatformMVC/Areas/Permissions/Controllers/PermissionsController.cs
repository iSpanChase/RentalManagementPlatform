using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Permissions.PermissionsDTOs;
using RentalManagementPlatformMVC.Areas.Permissions.Services;
using RentalManagementPlatformMVC.Areas.Permissions.ViewModels;
using System.Linq;

namespace RentalManagementPlatformMVC.Areas.Permissions.Controllers
{
	[Area("Permissions")]
	[Authorize]
	public class PermissionsController : Controller
	{
		private readonly IPermissionsService _svc;
		public PermissionsController(IPermissionsService svc) => _svc = svc;

		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] PermissionQueryInput input)
		{
			var paged = await _svc.QueryAsync(input);
			var items = paged.Items?.ToList() ?? new List<PermissionListItemDto>();
			var vm = new PermissionsIndexVm
			{
				Items = items,
				Query = input,
				Pagination = new()
				{
					Page = input.Page,
					PageSize = input.PageSize,
					Total = paged.Total
				},
				// 可選：提供現有的模組/動作清單（用於下拉）
				Modules = items
			.Select(i => i.Module)
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct()
			.OrderBy(s => s)
			.ToList(),

				Actions = items
			.Select(i => i.Action)
			.Where(s => !string.IsNullOrWhiteSpace(s))
			.Distinct()
			.OrderBy(s => s)
			.ToList()
			};
			return View(vm);
		}

		// Create
		[HttpGet]
		public IActionResult Create() => View(new PermissionFormVm());

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(PermissionFormVm vm)
		{
			if (!ModelState.IsValid) return View(vm);

			try
			{
				var id = await _svc.CreateAsync(new CreatePermissionDto
				{
					PermCode = vm.PermCode,
					PermName = vm.PermName,
					Module = vm.Module,
					Action = vm.Action,
					Description = vm.Description
				});
				return RedirectToAction(nameof(Details), new { id });
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(vm);
			}
		}

		// Edit
		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var d = await _svc.GetDetailAsync(id);
			if (d is null) return NotFound();

			var vm = new PermissionFormVm
			{
				PermissionId = d.PermissionId,
				PermCode = d.PermCode,
				PermName = d.PermName,
				Module = d.Module,
				Action = d.Action,
				Description = d.Description
			};
			return View(vm);
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(PermissionFormVm vm)
		{
			if (!ModelState.IsValid) return View(vm);
			if (vm.PermissionId is null || vm.PermissionId <= 0) return BadRequest("Missing permission id.");

			try
			{
				await _svc.UpdateAsync(vm.PermissionId.Value, new UpdatePermissionDto
				{
					PermissionId = vm.PermissionId.Value,
					PermCode = vm.PermCode,
					PermName = vm.PermName,
					Module = vm.Module,
					Action = vm.Action,
					Description = vm.Description
				});
				return RedirectToAction(nameof(Details), new { id = vm.PermissionId });
			}
			catch (KeyNotFoundException)
			{
				return NotFound();
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError(string.Empty, ex.Message);
				return View(vm);
			}
		}

		// Details
		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var d = await _svc.GetDetailAsync(id);
			if (d is null) return NotFound();
			return View(d);
		}

		// Delete
		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _svc.DeleteAsync(id);
			TempData["Msg"] = "Permission 已刪除";
			return RedirectToAction(nameof(Index));
		}

		// 唯讀：查看哪些角色擁有此權限（不做指派）
		[HttpGet]
		public async Task<IActionResult> UsedByRoles(int id)
		{
			var vm = await _svc.GetUsedByRolesVmAsync(id);
			return View(vm);
		}
	}
}
