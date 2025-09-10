using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Payments.ViewModels;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Services;

namespace RentalManagementPlatformMVC.Areas.Payments.Controllers
{
	[Area("Payments")]
	public class HostPayoutHomeController : Controller
	{
		private readonly IHostPayoutService _hostPayoutService;

		public HostPayoutHomeController(IHostPayoutService hostPayoutService)
		{
			_hostPayoutService = hostPayoutService;
		}

		[HttpGet]
		public async Task<IActionResult> Index(HostSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
		{
			var paged = await _hostPayoutService.GetPagedHostPayoutsAsync(pageIndex, pageSize);

			var vm = new HostPayoutIndexViewModel
			{
				HostPayouts = paged.Items.Select(p => new HostPayoutIndexRowViewModel
				{
					PayoutId = p.PayoutId,
					HostId = p.HostId,
					//HostName = p.HostName,
					CycleStart = p.CycleStart,
					CycleEnd = p.CycleEnd,
					PaidAt = p.PaidAt,
					Status = p.Status,
					CreatedAt = p.CreatedAt,
				}).ToList(),

				PageIndex = paged.PageIndex,
				PageSize = paged.PageSize,
				TotalCount = paged.TotalCount,
				TotalPages = paged.TotalPages,
			};

			ViewData["ActiveTab"] = "host";
			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> Details(int hostPayoutId)
		{
			var hostPayout = await _hostPayoutService.GetHostPayoutByIdAsync(hostPayoutId);
			if (hostPayout == null) return NotFound();

			var vm = new HostPayoutDetailViewModel
			{
				PayoutId = hostPayout.PayoutId,
				HostId = hostPayout?.HostId,
				//HostName = payout.HostName,
				CycleStart = hostPayout?.CycleStart,
				CycleEnd = hostPayout?.CycleEnd,
				AmountGross = hostPayout?.AmountGross,
				PlatformFee = hostPayout?.PlatformFee,
				AmountNet = hostPayout?.AmountNet,
				PaidAt = hostPayout.PaidAt,
				Status = hostPayout.Status,
				CreatedAt = hostPayout.CreatedAt,
				Items = hostPayout.Items?
							.OrderByDescending(i => i.BookingId)
							.ToList() ?? new()
			};

			return PartialView("_HostPayoutDetailsPartial", vm);
		}
	}
}
