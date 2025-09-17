using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;
using RentalManagementPlatformMVC.Exceptions;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Areas.SubscriptionPlan.Controllers
{
	[Area("SubscriptionPlan")]
	[Authorize]
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

			ViewData["ActiveTab"] = "plan";
			return View(vm);
		}

		/// <summary>
		/// 建立新的訂閱方案（僅支援 AJAX）。
		/// </summary>
		/// <param name="viewModel">包含訂閱方案資料的 ViewModel。</param>
		/// <returns>JSON 格式的建立結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(CreatePlanViewModel viewModel)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.ToDictionary(
						kvp => kvp.Key,
						kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
					);
				return Json(new { success = false, errors = errors });
			}

			try
			{
				var createPlanDto = _mapper.Map<CreatePlanDto>(viewModel);
				await _subscriptionPlanService.CreatePlanAsync(createPlanDto);
				
				return Json(new { success = true, message = "方案創建成功！" });
			}
			catch (PlanNameExistException ex)
			{
				return Json(new { success = false, error = ex.Message, field = nameof(viewModel.PlanName) });
			}
			catch (ArgumentException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, error = "創建方案時發生錯誤，請稍後再試！" });
			}
		}

		/// <summary>
		/// 刪除指定方案（僅支援 AJAX）。
		/// </summary>
		/// <param name="planId">指定方案的Id</param>
		/// <returns>JSON 格式的刪除結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int planId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _subscriptionPlanService.DeletePlanAsync(planId);

				if (success)
				{
					return Json(new { success = true, message = "方案刪除成功！" });
				}
				else
				{
					return Json(new { success = false, error = "方案刪除失敗，請稍後再試！" });
				}
			}
			catch (PlanNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PlanInUseException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, error = "刪除方案時發生錯誤，請稍後再試！" });
			}
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(EditPlanViewModel viewModel)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			if (!ModelState.IsValid)
			{
				var errors = ModelState
					.Where(x => x.Value.Errors.Count > 0)
					.ToDictionary(
						kvp => kvp.Key,
						kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
					);
				return Json(new { success = false, errors = errors });
			}

			try
			{
				var updateDto = _mapper.Map<EditPlanDto>(viewModel);
				await _subscriptionPlanService.EditPlanAsync(updateDto);
				return Json(new { success = true, message = "方案更新成功！" });
			}
			catch (PlanNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PlanNameExistException ex)
			{
				return Json(new { success = false, error = ex.Message, field = nameof(viewModel.PlanName) });
			}
			catch (ArgumentException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, error = "更新方案時發生錯誤，請稍後再試！" });
			}
		}

		/// <summary>
		/// 啟用指定方案（僅支援 AJAX）。
		/// </summary>
		/// <param name="planId">指定方案的Id</param>
		/// <returns>JSON 格式的啟用結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Activate(int planId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _subscriptionPlanService.ActivatePlanAsync(planId);

				if (success)
				{
					return Json(new { success = true, message = "方案啟用成功！" });
				}
				else
				{
					return Json(new { success = false, error = "方案啟用失敗，請稍後再試！" });
				}
			}
			catch (PlanNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, error = "啟用方案時發生錯誤，請稍後再試！" });
			}
		}

		/// <summary>
		/// 停用指定方案（僅支援 AJAX）。
		/// </summary>
		/// <param name="planId">指定方案的Id</param>
		/// <returns>JSON 格式的停用結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deactivate(int planId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _subscriptionPlanService.DeactivatePlanAsync(planId);

				if (success)
				{
					return Json(new { success = true, message = "方案停用成功！" });
				}
				else
				{
					return Json(new { success = false, error = "方案停用失敗，請稍後再試！" });
				}
			}
			catch (PlanNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PlanInUseException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception)
			{
				return Json(new { success = false, error = "停用方案時發生錯誤，請稍後再試！" });
			}
		}
	}
}
