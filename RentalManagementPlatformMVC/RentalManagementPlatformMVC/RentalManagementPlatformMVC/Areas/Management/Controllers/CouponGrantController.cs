using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Controllers
{
	[Area("Management")]
	[Authorize]
	public class CouponGrantController : Controller
	{
		private readonly CouponGrantService _grantService;
		private readonly CouponDropdownService _couponDropdownService;
		private readonly UserDropdownService _userDropdownService;
		private readonly RentalManagementPlatformSqlContext _db;

		public CouponGrantController(
			CouponGrantService grantService,
			CouponDropdownService couponDropdownService,
			UserDropdownService userDropdownService,
			RentalManagementPlatformSqlContext db)
		{
			_grantService = grantService;
			_couponDropdownService = couponDropdownService;
			_userDropdownService = userDropdownService;
			_db = db;
		}

		// GET 顯示發放表單
		[HttpGet]
		public async Task<IActionResult> Grant()
		{
			var vm = new CouponGrantVm();
			await PopulateDropdowns(vm);
			return View(vm);
		}

		// POST 發放
		[HttpPost]
		public async Task<IActionResult> Grant(CouponGrantVm vm)
		{
			// 驗證優惠券
			if (vm.SelectedCouponId <= 0)
			{
				ModelState.Remove("SelectedCouponId");
				ModelState.AddModelError(nameof(vm.SelectedCouponId), "請選擇優惠券");
			}

			// 驗證使用者
			if (vm.SelectedUserIds<=0)
			{
				ModelState.Remove("SelectedUserIds");
				ModelState.AddModelError(nameof(vm.SelectedUserIds), "請選擇使用者");
			}

			if (!ModelState.IsValid)
			{
				await PopulateDropdowns(vm);
				return View(vm);
			}

			bool alreadyGranted = _db.CouponGuests.Any(cg => cg.CouponId == vm.SelectedCouponId && cg.GuestId == vm.SelectedUserIds);

			if (alreadyGranted)
			{
				ModelState.AddModelError("", "使用者已經擁有此優惠券");
				await PopulateDropdowns(vm);
				return View(vm);
			}

			_db.CouponGuests.Add(new CouponGuest
			{
				CouponGuestId = (_db.CouponGuests.Max(cg => (int?)cg.CouponGuestId) ?? 0) + 1,
				CouponId = vm.SelectedCouponId,
				GuestId = vm.SelectedUserIds,
				CreateAt = DateTime.Now
			});

			await _db.SaveChangesAsync();
			ViewData["Message"] = "優惠券發放成功！";
			await PopulateDropdowns(vm);
			return View(vm);
		}

		// 初始化下拉選單
		private async Task PopulateDropdowns(CouponGrantVm vm)
		{
			vm.Coupons = (await _couponDropdownService.GetAvailableCouponsAsync())
				.Select(c => new SelectListItem($"{c.CouponId}-{c.CouponName}", c.CouponId.ToString()))
				.ToList();

			vm.Users = (await _userDropdownService.GetAllUsersAsync())
				.Select(u => new SelectListItem($"{u.UserId}-{u.Name}", u.UserId.ToString()))
				.ToList();
		}
	}
}
