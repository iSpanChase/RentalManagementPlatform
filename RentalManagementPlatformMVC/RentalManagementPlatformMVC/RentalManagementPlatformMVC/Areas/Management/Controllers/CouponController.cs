using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Management.Services;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.ViewModels;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Controllers;

[Area("Management")]
public class CouponController : Controller
{
	private readonly ICouponQueryService _querySvc;
	private readonly CouponCommandService _commandSvc;

	public CouponController(ICouponQueryService querySvc, CouponCommandService commandSvc)
	{
		_querySvc = querySvc;
		_commandSvc = commandSvc;
	}

	public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? keyword = null)
	{
		var (data, totalCount) = await _querySvc.GetPagedListAsync(page, pageSize, keyword);

		var vmList = data.Select(d => new CouponListItemVm
		{
			CouponId = d.CouponId,
			CouponName = d.CouponName,
			DiscountCode = d.DiscountCode,
			Description = d.Description,
			MinRentalPeriod = d.MinRentalPeriod,
			MaxRentalPeriod = d.MaxRentalPeriod,
			StartRentalPeriod = d.StartRentalPeriod,
			EndRentalPeriod = d.EndRentalPeriod,
			DiscountMethod = d.DiscountMethod,
			DiscountQuota = d.DiscountQuota,
			LowSpend = d.LowSpend,
			EndAt = d.EndAt,
			IsDeleted = d.IsDeleted
		}).ToList();

		ViewData["TotalCount"] = totalCount;
		ViewData["PageIndex"] = page;
		ViewData["PageSize"] = pageSize;
		ViewData["Keyword"] = keyword;

		return View(vmList);
	}


	[HttpPost]
	public async Task<IActionResult> Create(Coupon coupon)
	{
		await _commandSvc.AddCouponAsync(coupon);
		return RedirectToAction("Index");
	}

	[HttpPost]
	public async Task<IActionResult> Edit(Coupon coupon)
	{
		await _commandSvc.UpdateCouponAsync(coupon);
		return RedirectToAction("Index");
	}

	[HttpPost]
	public async Task<IActionResult> Delete(int id)
	{
		await _commandSvc.SoftDeleteCouponAsync(id);
		return RedirectToAction("Index");
	}
}
