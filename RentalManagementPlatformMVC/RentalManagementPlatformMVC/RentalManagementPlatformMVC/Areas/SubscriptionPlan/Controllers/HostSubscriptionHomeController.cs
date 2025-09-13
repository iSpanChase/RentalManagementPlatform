using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;
using System.Linq;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.Controllers
{
	[Area("SubscriptionPlan")]
	public class HostSubscriptionHomeController : Controller
	{
		private readonly IHostSubscriptionService _hostSubscriptionService;
		private readonly IMapper _mapper;

		public HostSubscriptionHomeController(IHostSubscriptionService hostSubscriptionService, IMapper mapper)
		{
			_hostSubscriptionService = hostSubscriptionService;
			_mapper = mapper;
		}

		[HttpGet]
		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
		{
			var pagedResult = await _hostSubscriptionService.GetPagedHostSubscriptionsAsync(pageIndex, pageSize);

			var vm = new HostSubscriptionIndexViewModel
			{
				HostSubscriptions = _mapper.Map<List<HostSubscriptionIndexRowViewModel>>(pagedResult.Items),

				PageIndex = pagedResult.PageIndex,
				TotalPages = pagedResult.TotalPages,
				PageSize = pagedResult.PageSize,
				TotalCount = pagedResult.TotalCount
			};

			ViewData["ActiveTab"] = "host";
			return View(vm);
		}
	}
}
