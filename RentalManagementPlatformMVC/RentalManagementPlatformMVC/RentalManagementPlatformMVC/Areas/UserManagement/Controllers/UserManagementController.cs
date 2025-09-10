using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementPlatformMVC.Areas.UserManagement.Mapping;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Controllers
{
	/// <summary>
	/// 會員管理（User）後台操作的 MVC 控制器。
	/// 僅負責接收/回傳 ViewModel，商業邏輯委派至服務層。
	/// </summary>
	[Area("UserManagement")]
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
		public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = DefaultPageSize)
		{
			var (items, total) = await _svc.ListAsync(q, page, pageSize);

			var vm = new UserIndexVm
			{
				Items = items.Select(x => x.ToVm()).ToList(),
				Query = q,
				CurrentPage = page,
				PageSize = pageSize,
				Total = total
			};
			return View(vm);
		}

		/// <summary>
		/// 顯示新增使用者表單。
		/// </summary>
		/// <returns>Create 視圖。</returns>
		[HttpGet]
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
		public async Task<IActionResult> Edit(int id)
		{
			var detail = await _svc.GetAsync(id);
			var vm = detail.ToEditVm();


			if (detail == null) return NotFound();

			// 建立選項（固定 M/F）
			var genders = new List<SelectListItem>
			{
				new SelectListItem { Value = "M", Text = "男", Selected = (detail.Gender == "M") },
				new SelectListItem { Value = "F", Text = "女", Selected = (detail.Gender == "F") }
			};
			ViewBag.Genders = genders;

			return View(vm);
		}

		/// <summary>
		/// 接收編輯表單並更新資料。
		/// </summary>
		/// <param name="vm">使用者編輯表單的 ViewModel。</param>
		/// <returns>成功導回清單；失敗則回填表單。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserEditVm vm)
		{
			if (!ModelState.IsValid) return View(vm);

			await _svc.UpdateAsync(vm.ToDto());
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
		public async Task<IActionResult> Delete(int id)
		{
			await _svc.DeleteAsync(id);
			TempData["ok"] = "刪除成功";
			return RedirectToAction(nameof(Index));
		}
	}
}
