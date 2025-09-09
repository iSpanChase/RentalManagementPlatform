using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementPlatformMVC.Areas.UserManagement.Mapping;
using RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.UserManagement.Controllers
{
	[Area("UserManagement")]
	public class UserManagementController : Controller
	{
		private readonly IUserService _svc; // MVC Facade
		private const int DefaultPageSize = 10;

		public UserManagementController(IUserService svc) => _svc = svc;

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

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Edit(UserEditVm vm)
		{
			if (!ModelState.IsValid) return View(vm);

			await _svc.UpdateAsync(vm.ToDto());
			TempData["ok"] = "更新成功";
			return RedirectToAction(nameof(Index));
		}

		public async Task<IActionResult> Delete(int id)
		{
			await _svc.DeleteAsync(id);
			TempData["ok"] = "刪除成功";
			return RedirectToAction(nameof(Index));
		}
	}
}
