using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementPlatformMVC.Areas.Permissions.Models;
using RentalManagementPlatformMVC.Areas.UserManagement.Mapping;
using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.UserServices;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Controllers
{
	/// <summary>
	/// 會員管理（User）後台操作的 MVC 控制器。
	/// 僅負責接收/回傳 ViewModel，商業邏輯委派至服務層。
	/// </summary>
	[Area("UserManagement")]
	[Authorize]
	public class UserManagementController : Controller
	{
		private readonly IUserService _svc; // MVC Facade
		private const int DefaultPageSize = 10;

		/// <summary>
		/// 以 DI 注入使用者服務。
		/// </summary>
		/// <param name="svc">使用者服務介面（Facade）。</param>
		public UserManagementController(IUserService svc) => _svc = svc;

		/// <summary>
		/// 使用者清單頁。支援關鍵字查詢與分頁。
		/// </summary>
		/// <param name="q">關鍵字（帳號/Email/姓名）。</param>
		/// <param name="page">頁碼（1 起算）。</param>
		/// <param name="pageSize">每頁筆數。</param>
		/// <returns>清單頁的 View 結果。</returns>
		[HttpGet]
		[Authorize(Policy = AppPermissions.Users.Browse)]
		public async Task<IActionResult> Index([FromQuery] UserFilterVm f)
		{
			var (items, total) = await _svc.ListAsync(f);

			var vm = new UserIndexVm
			{
				Items = items.Select(d => new UserListItemVm
				{
					UserId = d.UserId,
					Username = d.Username,
					Email = d.Email,
					Name = d.Name,
					CreatedAt = d.CreatedAt
				}).ToList(),
				CurrentPage = f.Page,
				PageSize = f.PageSize,
				Total = total,
				SortBy = f.SortBy,
				SortDir = f.SortDir,
				Filter = f
			};
			return View(vm);
		}

		/// <summary>
		/// 顯示新增使用者表單。
		/// </summary>
		/// <returns>Create 視圖。</returns>
		[HttpGet]
		[Authorize(Policy = AppPermissions.Users.Create)]
		public IActionResult Create() 
		{
			var genders = new List<SelectListItem>
			{
				new SelectListItem { Value = "M", Text = "男" },
				new SelectListItem { Value = "F", Text = "女" }
			};
			ViewBag.Genders = genders;

			return View(new UserCreateVm()); // 帶一個空的 Model 給表單
		}

		/// <summary>
		/// 接收新增使用者表單並建立資料。
		/// </summary>
		/// <param name="vm">使用者建立表單的 ViewModel。</param>
		/// <returns>成功導回清單；失敗則回填表單。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = AppPermissions.Users.Create)]
		public async Task<IActionResult> Create(UserCreateVm vm)
		{
			if (!ModelState.IsValid) return View(vm);

			try
			{
				await _svc.CreateAsync(vm.ToDto());
				TempData["ok"] = "建立成功";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View(vm);
			}
		}

		/// <summary>
		/// 顯示編輯使用者表單。
		/// </summary>
		/// <param name="id">使用者主鍵。</param>
		/// <returns>Edit 視圖。</returns>
		[HttpGet]
		[Authorize(Policy = AppPermissions.Users.Edit)]
		public async Task<IActionResult> Edit(int id)
		{
			var detail = await _svc.GetAsync(id);
			if (detail == null) return NotFound();         // 先檢查再使用

			ViewBag.Genders = BuildGenderItems(detail.Gender);

			var vm = new UserEditVm
			{
				UserId = detail.UserId,
				Username = detail.Username,
				Email = detail.Email,
				Name = detail.Name,
				Gender = detail.Gender,
				BirthDate = detail.BirthDate,   
				Phone = detail.Phone,
				Address = detail.Address,
				Point = detail.Point,
				Isverified = detail.Isverified,           
				ProfileImageurl = detail.ProfileImageurl
			};
			return View(vm);
		}

		/// <summary>
		/// 接收編輯表單並更新資料。
		/// </summary>
		/// <param name="vm">使用者編輯表單的 ViewModel。</param>
		/// <returns>成功導回清單；失敗則回填表單。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = AppPermissions.Users.Edit)]
		public async Task<IActionResult> Edit(UserEditVm vm)
		{
			// 觀察是否真的打進來
			Console.WriteLine($"[POST Edit] HIT, UserId={vm.UserId}");

			if (!ModelState.IsValid)
			{
				ViewBag.Genders = BuildGenderItems(vm.Gender);  // 重新準備下拉資料
																// 列出所有欄位的錯誤（Console）
				foreach (var kv in ModelState)                  // 方便開發時觀察錯誤內容
				{
					var key = kv.Key;
					var errors = kv.Value?.Errors;
					if (errors != null && errors.Count > 0)
					{
						foreach (var e in errors)
						{
							Console.WriteLine($"[ModelError] {key}: {e.ErrorMessage}");
						}
					}
				}
				// 丟到 TempData 方便在畫面上看到錯誤訊息（可選）
				TempData["modelErrors"] = string.Join(" | ",
					ModelState.Where(kv => kv.Value!.Errors.Any())
							  .SelectMany(kv => kv.Value!.Errors.Select(e => $"{kv.Key}:{e.ErrorMessage}")));

				return View(vm);
			}

			await _svc.UpdateAsync(new UpdateUserDto
			{
				UserId = vm.UserId,
				Email = vm.Email,
				Name = vm.Name,
				Gender = vm.Gender,
				BirthDate = vm.BirthDate,       
				Phone = vm.Phone,
				Address = vm.Address,
				Point = vm.Point,
				Isverified = vm.Isverified,     
				ProfileImageurl = vm.ProfileImageurl
			});

			TempData["ok"] = "更新成功";
			return RedirectToAction(nameof(Index));
		}

		/// <summary>
		/// 刪除指定使用者。
		/// </summary>
		/// <param name="id">使用者主鍵。</param>
		/// <returns>導回清單頁。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = AppPermissions.Users.Delete)]
		public async Task<IActionResult> Delete(int id)
		{
			await _svc.DeleteAsync(id);
			TempData["ok"] = "刪除成功";
			return RedirectToAction(nameof(Index));
		}

		// 供 GET/POST 共用，避免重複碼
		private static List<SelectListItem> BuildGenderItems(string? selected) => new()
		{
			new SelectListItem { Value = "M", Text = "男", Selected = selected == "M" },
			new SelectListItem { Value = "F", Text = "女", Selected = selected == "F" },
		};

		/// <summary>
		/// 顯示角色指派表單。
		/// </summary>
		/// <param name="id">使用者主鍵。</param>
		/// <returns>AssignRoles 視圖。</returns>
		[HttpGet]
		[Authorize(Policy = AppPermissions.Users.AssignRoles)]
		public async Task<IActionResult> AssignRoles(int id)
		{
			var vm = await _svc.GetAssignRolesAsync(id);
			return View(vm);
		}

		/// <summary>
		/// 接收角色指派表單並更新資料。
		/// </summary>
		/// <param name="vm">角色指派表單的 ViewModel。</param>
		/// <returns>成功導回清單；失敗則回填表單。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		[Authorize(Policy = AppPermissions.Users.AssignRoles)]
		public async Task<IActionResult> AssignRoles(AssignUserRolesVm vm)
		{
			await _svc.AssignRolesAsync(vm.UserId, vm.SelectedRoleIds ?? Array.Empty<int>());
			TempData["Msg"] = "角色指派已更新";
			return RedirectToAction(nameof(Index)); // 導回使用者清單或明細
		}
	}
}
