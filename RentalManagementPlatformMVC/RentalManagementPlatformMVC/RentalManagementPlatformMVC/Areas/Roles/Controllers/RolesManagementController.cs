using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Roles.Mapping;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.Areas.Roles.RolesServices;
using RentalManagementPlatformMVC.Areas.Roles.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Roles.Controllers
{
	[Area("Roles")]
	[Authorize]
	public class RolesManagementController : Controller
	{
		private readonly IRolesService _svc;
		public RolesManagementController(IRolesService svc) => _svc = svc;

		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] RoleQueryInput input)
		{
			var paged = await _svc.QueryAsync(input);
			var vm = new RolesIndexVm
			{
				Items = paged.Items,
				Query = input,
				Pagination = new PaginationVm
				{
					Page = input.Page,
					PageSize = input.PageSize,
					Total = paged.Total
				}
			};
			return View(vm);
		}

		// Create
		public IActionResult Create() => View(new RoleFormVm());

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(RoleFormVm vm)
		{
			if (!ModelState.IsValid) return View(vm);
			await _svc.CreateAsync(vm.ToCreateDto());
			return RedirectToAction(nameof(Index));
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var detail = await _svc.GetDetailAsync(id);
			if (detail is null) return NotFound();

			var vm = detail.ToFormVm(id); // 這裡會把 RoleId = id
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(RoleFormVm vm)
		{
			if (!ModelState.IsValid) return View(vm);
			if (vm.RoleId is null || vm.RoleId <= 0) return BadRequest("Missing role id.");

			try
			{
				await _svc.UpdateAsync(vm.RoleId.Value, vm.ToUpdateDto());
				return RedirectToAction(nameof(Index));
			}
			catch (KeyNotFoundException)
			{
				return NotFound(); // 或者顯示一個友善訊息
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int id)
		{
			await _svc.DeleteAsync(id);
			TempData["Msg"] = "角色已刪除";
			return RedirectToAction(nameof(Index));
		}

		// 指派使用者
		public async Task<IActionResult> AssignUsers(int id)
		{
			var vm = await _svc.GetAssignUsersVmAsync(id);
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignUsers(AssignUsersVm vm)
		{
			await _svc.SaveAssignUsersAsync(vm);
			TempData["Msg"] = "已更新角色成員";
			return RedirectToAction(nameof(Edit), new { id = vm.RoleId });
		}

		// 指派權限
		public async Task<IActionResult> AssignPermissions(int id)
		{
			var vm = await _svc.GetAssignPermissionsVmAsync(id);
			return View(vm);
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignPermissions(AssignPermissionsVm vm)
		{
			await _svc.SaveAssignPermissionsAsync(vm);
			TempData["Msg"] = "已更新角色權限";
			return RedirectToAction(nameof(Edit), new { id = vm.RoleId });
		}
	}
}
