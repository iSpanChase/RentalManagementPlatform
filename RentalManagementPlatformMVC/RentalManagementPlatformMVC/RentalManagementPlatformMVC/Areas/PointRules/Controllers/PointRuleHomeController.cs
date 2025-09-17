using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Exceptions;
using RentalManagementPlatformMVC.Services.PointRules;

namespace RentalManagementPlatformMVC.Areas.PointRules.Controllers
{
	[Area("PointRules")]
	public class PointRuleHomeController : Controller
	{
		private readonly IPointRuleService _pointRuleService;
		private readonly IMapper _mapper;

		public PointRuleHomeController(IPointRuleService pointRuleService, IMapper mapper)
		{
			_pointRuleService = pointRuleService;
			_mapper = mapper;
		}

		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
		{
			var pagedResult = await _pointRuleService.GetPagedPointRulesAsync(pageIndex, pageSize);

			var vm = new ViewModels.PointRuleIndexViewModel
			{
				Rules = _mapper.Map<List<ViewModels.PointRuleIndexRowViewModel>>(pagedResult.Items),
				PageIndex = pagedResult.PageIndex,
				PageSize = pagedResult.PageSize,
				TotalPages = pagedResult.TotalPages,
				TotalCount = pagedResult.TotalCount
			};

			ViewData["ActiveTab"] = "rule";
			return View(vm);
		}

		/// <summary>
		/// 建立新的點數規則（僅支援 AJAX）。
		/// </summary>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Create(ViewModels.CreatePointRuleViewModel createViewModel)
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
				var createDto = _mapper.Map<CreatePointRuleDto>(createViewModel);
				await _pointRuleService.CreatePointRuleAsync(createDto);
				return Json(new { success = true, message = "點數規則創建成功！" });
			}
			catch (InvalidPointRuleDataException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PointRuleDateRangeException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (ArgumentException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
		}

		/// <summary>
		/// 刪除指定點數規則（僅支援 AJAX）。
		/// </summary>
		/// <param name="ruleId">指定點數規則的Id</param>
		/// <returns>JSON 格式的刪除結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Delete(int ruleId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _pointRuleService.DeletePointRuleAsync(ruleId);

				if (success)
				{
					return Json(new { success = true, message = "點數規則刪除成功！" });
				}
				else
				{
					return Json(new { success = false, error = "點數規則刪除失敗，請稍後再試！" });
				}
			}
			catch (PointRuleNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PointRuleInUseException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception ex)
			{
				var errorDetails = $"刪除點數規則時發生錯誤: {ex.Message}";
				if (ex.InnerException != null)
				{
					errorDetails += $" | 內部錯誤: {ex.InnerException.Message}";
				}
				return Json(new { success = false, error = errorDetails });
			}
		}

		/// <summary>
		/// 編輯指定點數規則（僅支援 AJAX）。
		/// </summary>
		/// <param name="editViewModel">包含點數規則編輯資料的 ViewModel。</param>
		/// <returns>JSON 格式的編輯結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Update(ViewModels.EditPointRuleViewModel editViewModel)
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
				var editDto = _mapper.Map<DTOs.PointRules.EditPointRuleDto>(editViewModel);
				await _pointRuleService.EditPointRuleAsync(editDto);
				return Json(new { success = true, message = "點數規則更新成功！" });
			}
			catch (PointRuleNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidPointRuleDataException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PointRuleDateRangeException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (ArgumentException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception ex)
			{
				var errorDetails = $"更新點數規則時發生錯誤: {ex.Message}";
				if (ex.InnerException != null)
				{
					errorDetails += $" | 內部錯誤: {ex.InnerException.Message}";
				}
				return Json(new { success = false, error = errorDetails });
			}
		}

		/// <summary>
		/// 啟用指定點數規則（僅支援 AJAX）。
		/// </summary>
		/// <param name="ruleId">指定點數規則的Id</param>
		/// <returns>JSON 格式的啟用結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Activate(int ruleId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _pointRuleService.ActivatePointRuleAsync(ruleId);

				if (success)
				{
					return Json(new { success = true, message = "點數規則啟用成功！" });
				}
				else
				{
					return Json(new { success = false, error = "點數規則啟用失敗，請稍後再試！" });
				}
			}
			catch (PointRuleNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception ex)
			{
				var errorDetails = $"啟用點數規則時發生錯誤: {ex.Message}";
				if (ex.InnerException != null)
				{
					errorDetails += $" | 內部錯誤: {ex.InnerException.Message}";
				}
				return Json(new { success = false, error = errorDetails });
			}
		}

		/// <summary>
		/// 停用指定點數規則（僅支援 AJAX）。
		/// </summary>
		/// <param name="ruleId">指定點數規則的Id</param>
		/// <returns>JSON 格式的停用結果。</returns>
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Deactivate(int ruleId)
		{
			// 僅支援 AJAX 請求
			if (Request.Headers["X-Requested-With"] != "XMLHttpRequest")
			{
				return BadRequest(new { success = false, error = "只支援 AJAX 請求。" });
			}

			try
			{
				var success = await _pointRuleService.DeactivatePointRuleAsync(ruleId);

				if (success)
				{
					return Json(new { success = true, message = "點數規則停用成功！" });
				}
				else
				{
					return Json(new { success = false, error = "點數規則停用失敗，請稍後再試！" });
				}
			}
			catch (PointRuleNotFoundException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (PointRuleInUseException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (InvalidOperationException ex)
			{
				return Json(new { success = false, error = ex.Message });
			}
			catch (Exception ex)
			{
				var errorDetails = $"停用點數規則時發生錯誤: {ex.Message}";
				if (ex.InnerException != null)
				{
					errorDetails += $" | 內部錯誤: {ex.InnerException.Message}";
				}
				return Json(new { success = false, error = errorDetails });
			}
		}

	}
}
