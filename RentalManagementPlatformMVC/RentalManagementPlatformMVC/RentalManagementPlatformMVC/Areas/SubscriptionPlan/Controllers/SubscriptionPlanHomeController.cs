using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;
using RentalManagementPlatformMVC.Exceptions;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.Controllers
{
	[Area("SubscriptionPlan")]
	public class SubscriptionPlanHomeController : Controller
	{
		private readonly ISubscriptionPlanService _subscriptionPlanService;
		private readonly IMapper _mapper;

		public SubscriptionPlanHomeController(ISubscriptionPlanService subscriptionPlanService, IMapper mapper)
		{
			_subscriptionPlanService = subscriptionPlanService;
			_mapper = mapper;
		}

		/// <summary>
		/// 取得分頁的訂閱方案列表。
		/// </summary>
		/// <param name="pageIndex">目前頁碼，預設為 1。</param>
		/// <param name="pageSize">每頁顯示筆數，預設為 20。</param>
		/// <returns>分頁的訂閱方案檢視畫面。</returns>
		[HttpGet]
		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
		{
			var pagedResult = await _subscriptionPlanService.GetPagedPlansAsync(pageIndex, pageSize);

			var vm = new SubscriptionPlanIndexViewModel
			{
				Plans = _mapper.Map<List<SubscriptionPlanIndexRowViewModel>>(pagedResult.Items),
				PageIndex = pagedResult.PageIndex,
				PageSize = pagedResult.PageSize,
				TotalPages = pagedResult.TotalPages,
				TotalCount = pagedResult.TotalCount
			};

			return View(vm);
		}

		/// <summary>
		/// 建立新的訂閱方案。
		/// </summary>
		/// <param name="viewModel">包含訂閱方案資料的 ViewModel。</param>
		/// <returns>建立結果的檢視畫面或重新導向至列表頁。</returns>
		[HttpPost]
		public async Task<IActionResult> Create(CreatePlanViewModel viewModel)
		{
			if (!ModelState.IsValid)
			{
				return View(viewModel);
			}

			try
			{
				var createPlanDto = _mapper.Map<CreatePlanDto>(viewModel);
				await _subscriptionPlanService.CreatePlanAsync(createPlanDto);
				
				TempData["Success"] = "方案創建成功！";
				return RedirectToAction(nameof(Index));
			}
			catch (PlanNameExistException ex)
			{
				ModelState.AddModelError(nameof(viewModel.PlanName), ex.Message);
				return View(viewModel);
			}
			catch (ArgumentException ex)
			{
				ModelState.AddModelError("", ex.Message);
				return View(viewModel);
			}
			catch (Exception ex)
			{
				ModelState.AddModelError("", "創建方案時發生錯誤，請稍後再試");
				return View(viewModel);
			}
		}

		/// <summary>
		///	刪除指定方案。
		/// </summary>
		/// <param name="planId">指定方案的Id</param>
		/// <returns>建立結果的檢視畫面或重新導向至列表頁。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int planId)
		{
			// 檢查是否為 AJAX 請求
			bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

			try
			{
				var success = await _subscriptionPlanService.DeletePlanAsync(planId);

				if (success)
				{
					if (isAjax)
						return Json(new { success = true, message = "方案刪除成功！" });
					
					TempData["Success"] = "方案刪除成功！";
				}
				else
				{
					if (isAjax)
						return Json(new { success = false, error = "方案刪除失敗，請稍後再試！" });
					
					TempData["Error"] = "方案刪除失敗，請稍後再試！";
				}
			}
			catch (PlanNotFoundException ex)
			{
				if (isAjax)
					return Json(new { success = false, error = ex.Message });
				TempData["Error"] = ex.Message;
			}
			catch (PlanInUseException ex)
			{
				if (isAjax)
					return Json(new { success = false, error = ex.Message });
				TempData["Error"] = ex.Message;
			}
			catch (InvalidOperationException ex)
			{
				if (isAjax)
					return Json(new { success = false, error = ex.Message });
				TempData["Error"] = ex.Message;
			}
			catch (Exception ex)
			{
				if (isAjax)
					return Json(new { success = false, error = "刪除方案時發生錯誤，請稍後再試！" });
				TempData["Error"] = "刪除方案時發生錯誤，請稍後再試！";
			}

			return RedirectToAction(nameof(Index));
		}
	}
}
