using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Roles.Mapping;
using RentalManagementPlatformMVC.Areas.Roles.RolesDTOs;
using RentalManagementPlatformMVC.Areas.Roles.RolesRepositories;
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

		[HttpGet]
		//[Route("Roles/RolesManagement/AssignUsers/{roleId:int?}")]
		public async Task<IActionResult> AssignUsers([FromRoute] int? id, [FromRoute] int? roleId, [FromQuery] AssignUsersQueryInput q)
		{
			// 兼容三種來源：?RoleId=、/AssignUsers/{id}、/AssignUsers/{roleId}
			q.RoleId = q.RoleId > 0 ? q.RoleId : (roleId ?? id ?? 0);

			if (q.RoleId <= 0) return BadRequest("RoleId is required.");

			var key = $"AssignUsers_Selected_{q.RoleId}";
			var selectedSet = HttpContext.Session.GetHashSetInt(key);

			// 第一次進來（Session 還是空的）→ 從資料庫把「目前已指派的使用者」塞進 Session
			if (selectedSet.Count == 0)
			{
				var already = await _svc.GetUserIdsInRoleAsync(q.RoleId); // Service 補這個方法即可
				selectedSet = already.ToHashSet();
				HttpContext.Session.SetHashSetInt(key, selectedSet);
			}

			// 取得清單（照你原本的查詢/分頁）
			var result = await _svc.QueryAssignableUsersAsync(q);

			// 用 Session 的集合覆蓋每列的 Selected（避免只看當頁資料庫）
			var items = result.Items
				.Select(x => x with { Selected = selectedSet.Contains(x.UserId) })
				.ToList();
			result = new PagedResult<AssignableUserListItemDto> { Total = result.Total, Items = items };

			var roleName = await _svc.GetRoleNameAsync(q.RoleId);
			return View((roleName, q, result));

		}

		[HttpPost, ValidateAntiForgeryToken]
		public IActionResult RememberAssignUsersPage(AssignUsersQueryInput q, int[]? SelectedUserIds, int[] CurrentPageIds, int targetPage)
		{
			if (q.RoleId <= 0) return BadRequest("RoleId is required.");
			var key = $"AssignUsers_Selected_{q.RoleId}";
			var set = HttpContext.Session.GetHashSetInt(key);

			// 以這一頁的 id 清單為準，套用「本頁最新勾選」
			var selectedOnPage = new HashSet<int>(SelectedUserIds ?? Array.Empty<int>());
			foreach (var id in CurrentPageIds)
				if (selectedOnPage.Contains(id)) set.Add(id); else set.Remove(id);

			HttpContext.Session.SetHashSetInt(key, set);
			return RedirectToAction(nameof(AssignUsers), new { q.RoleId, q.Keyword, q.SortBy, q.Desc, Page = targetPage, q.PageSize });
		}

		[HttpPost, ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignUsers(AssignUsersVm vm, int[] CurrentPageIds, int[]? SelectedUserIds)
		{
			if (vm.RoleId <= 0) return BadRequest("RoleId is required.");
			// 先把本頁勾選變更合併到 Session
			var key = $"AssignUsers_Selected_{vm.RoleId}";
			var set = HttpContext.Session.GetHashSetInt(key);
			var selectedOnPage = new HashSet<int>(SelectedUserIds ?? Array.Empty<int>());
			foreach (var id in CurrentPageIds)
				if (selectedOnPage.Contains(id)) set.Add(id); else set.Remove(id);
			HttpContext.Session.SetHashSetInt(key, set);

			// 真正儲存：以 Session 中的全集合作為最終勾選清單
			await _svc.SaveAssignUsersAsync(new AssignUsersVm { RoleId = vm.RoleId, SelectedUserIds = set.ToList() });

			// 清掉暫存 & 回前頁
			HttpContext.Session.Remove(key);
			TempData["AlertSuccess"] = "已更新指派的使用者。";
			return RedirectToAction("Edit", new { id = vm.RoleId, area = "Roles" });
		}

		[HttpGet]
		public IActionResult CancelAssignUsers(int roleId)
		{
			var key = $"AssignUsers_Selected_{roleId}";
			HttpContext.Session.Remove(key); // 清掉暫存
			return RedirectToAction("Edit", "RolesManagement", new { area = "Roles", id = roleId });
		}

		// 指派權限
		[HttpGet]
		//[Route("Roles/RolesManagement/AssignPermissions/{roleId:int?}")]
		public async Task<IActionResult> AssignPermissions([FromRoute] int? id, [FromRoute] int? roleId, [FromQuery] AssignPermissionsQueryInput q)
		{
			q.RoleId = q.RoleId > 0 ? q.RoleId : (roleId ?? id ?? 0);
			if (q.RoleId <= 0) return BadRequest("RoleId is required.");

			var Key = $"AssignPerms_Selected_{q.RoleId}";
			var selectedSet = HttpContext.Session.GetHashSetInt(Key);

			// 第一次進來：用資料庫現況初始化 Session 全集
			if (selectedSet.Count == 0)
			{
				var alreadyIds = await _svc.GetPermissionIdsInRoleAsync(q.RoleId); // 請在 Service 端補這個方法
				selectedSet = alreadyIds.ToHashSet();
				HttpContext.Session.SetHashSetInt(Key, selectedSet);
			}

			// 清單查詢（你既有的分頁查詢）
			var result = await _svc.QueryAssignablePermissionsAsync(q);

			// 用 Session 覆蓋本頁每列的 Selected
			var patchedItems = result.Items
				.Select(x => x with { Selected = selectedSet.Contains(x.PermissionId) })
				.ToList();

			var patchedResult = new PagedResult<AssignablePermissionListItemDto>
			{
				Total = result.Total,
				Items = patchedItems
			};

			var roleName = await _svc.GetRoleNameAsync(q.RoleId);
			return View((roleName, q, patchedResult));
		}

		// ========== 2) POST：換頁前先記錄本頁勾選，然後 Redirect 到目標頁 ==========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public IActionResult RememberAssignPermissionsPage(AssignPermissionsQueryInput q, int[] CurrentPageIds, int[]? SelectedPermissionIds, int targetPage)
		{
			if (q.RoleId <= 0) return BadRequest("RoleId is required.");

			var Key = $"AssignPerms_Selected_{q.RoleId}";
			var set = HttpContext.Session.GetHashSetInt(Key);

			var selectedOnPage = new HashSet<int>(SelectedPermissionIds ?? Array.Empty<int>());

			// 以「本頁的 id 範圍」為基準，套用最新勾選
			foreach (var pid in CurrentPageIds)
			{
				if (selectedOnPage.Contains(pid)) set.Add(pid);
				else set.Remove(pid);
			}

			HttpContext.Session.SetHashSetInt(Key, set);

			return RedirectToAction(nameof(AssignPermissions), new
			{
				q.RoleId,
				q.Keyword,
				q.SortBy,
				q.Desc,
				Page = targetPage,
				q.PageSize
			});
		}

		// ========== 3) POST：最終儲存（合併本頁差異→以 Session 全集入庫→清 Session→回 Edit） ==========
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> AssignPermissions(AssignPermissionsVm vm, int[] CurrentPageIds, int[]? SelectedPermissionIds)
		{
			if (vm.RoleId <= 0) return BadRequest("RoleId is required.");

			var Key = $"AssignPerms_Selected_{vm.RoleId}";
			var set = HttpContext.Session.GetHashSetInt(Key);

			// 先把本頁差異合併進 Session
			var selectedOnPage = new HashSet<int>(SelectedPermissionIds ?? Array.Empty<int>());
			foreach (var pid in CurrentPageIds)
			{
				if (selectedOnPage.Contains(pid)) set.Add(pid);
				else set.Remove(pid);
			}
			HttpContext.Session.SetHashSetInt(Key, set);

			// 以 Session 的全集合為最終權限清單寫回資料庫
			var finalVm = new AssignPermissionsVm
			{
				RoleId = vm.RoleId,
				SelectedPermissionIds = set.ToList()
			};
			await _svc.SaveAssignPermissionsAsync(finalVm);

			// 清掉暫存並回到 Edit
			HttpContext.Session.Remove(Key);
			TempData["AlertSuccess"] = "已更新指派的權限。";
			return RedirectToAction("Edit", "RolesManagement", new { area = "Roles", id = vm.RoleId });
		}

		[HttpGet]
		public IActionResult CancelAssignPermissions(int roleId)
		{
			var key = $"AssignPerms_Selected_{roleId}";
			HttpContext.Session.Remove(key); // 清掉暫存
			return RedirectToAction("Edit", "RolesManagement", new { area = "Roles", id = roleId });
		}
	}
}
