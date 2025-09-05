using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.UserDTOs;
using RentalManagementPlatformMVC.Repositories;

namespace RentalManagementPlatformMVC.Areas.User.Controllers
{
	[Area("User")]
	public class UserController : Controller
	{
		private readonly IUserService _svc;
		private const int DefaultPageSize = 10;

		public UserController(IUserService svc) => _svc = svc;

		public async Task<IActionResult> Index(string? q, int page = 1, int pageSize = DefaultPageSize)
		{
			var (items, total) = await _svc.ListAsync(q, page, pageSize);
			ViewBag.Total = total;
			ViewBag.CurrentPage = page;
			ViewBag.PageSize = pageSize;
			ViewBag.Query = q;
			return View(items);
		}

		[HttpGet]
		public IActionResult Create() => View(new CreateUserDto());

		[HttpPost]
		public async Task<IActionResult> Create(CreateUserDto dto)
		{
			if (!ModelState.IsValid) return View(dto);
			try
			{
				await _svc.CreateAsync(dto);
				TempData["ok"] = "建立成功";
				return RedirectToAction(nameof(Index));
			}
			catch (InvalidOperationException ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View(dto);
			}
		}

		[HttpGet]
		public async Task<IActionResult> Edit(int id)
		{
			var x = await _svc.GetAsync(id);
			var vm = new UpdateUserDto
			{
				UserId = x.UserId,
				Email = x.Email,
				Name = x.Name,
				AutoSubscribe = x.AutoSubscribe,
				Gender = x.Gender,
				BirthDate = x.BirthDate,
				Phone = x.Phone,
				Address = x.Address,
				Point = x.Point,
				Isverified = x.Isverified,
				ProfileImageurl = x.ProfileImageurl,
			};
			ViewBag.Username = x.Username; // 顯示但不可改
			return View(vm);
		}

		[HttpPost]
		public async Task<IActionResult> Edit(UpdateUserDto dto)
		{
			if (!ModelState.IsValid) return View(dto);
			await _svc.UpdateAsync(dto);
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
