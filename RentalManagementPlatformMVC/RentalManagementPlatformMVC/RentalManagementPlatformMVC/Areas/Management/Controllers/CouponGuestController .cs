using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.ViewModels;

namespace RentalManagementPlatformMVC.Areas.Management.Controllers
{
	[Area("Management")]//指定controller屬於Management區域
	public class CouponGuestController : Controller
	{
		private readonly ICouponGuestQueryService _service;
		public CouponGuestController(ICouponGuestQueryService service)//建構子,使用依賴注入service層
		{
			_service = service;
		}

		//view顯示CouponGuest列表
		public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null)
		{
			//呼叫service層,取得dto列表與總筆數
			var (dtoList, totalCount) = await _service.GetPagedListAsync(page, pageSize,keyword);
			//將dto轉成viewmodel,供view顯示
			var vmList = dtoList.Select(dto => new CouponGuestVm
			{
				CouponGuestId = dto.CouponGuestId,
				CouponId = dto.CouponId,
				GuestId = dto.GuestId,
				CouponName = dto.CouponName,
				DiscountCode = dto.DiscountCode,
				Name = dto.Name,
				CreateAt = dto.CreateAt,
				RemoveAt = dto.RemoveAt
			}).ToList();

			ViewData["TotalCount"] = totalCount;
			ViewData["PageSize"] = pageSize;
			ViewData["PageIndex"] = page;
			ViewData["Keyword"]= keyword;

			//回傳view並傳入viewmodel列表
			return View(vmList);
		}
	}
}
