using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.SubscriptionPlan.ViewModels;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using RentalManagementPlatformMVC.Services.SubscriptionPlans;
using System.Linq;
using System.Text;

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

		/// <summary>
		/// 訂閱管理主頁面，根據是否有搜尋條件決定顯示初始清單或搜尋結果。
		/// 當有任何篩選條件或排序參數時，會執行搜尋功能；否則顯示預設的分頁清單。
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含房東姓名、方案名稱、狀態、日期區間及排序參數</param>
		/// <param name="pageIndex">目前頁碼，預設為第1頁</param>
		/// <param name="pageSize">每頁顯示筆數，預設為20筆</param>
		/// <returns>返回包含分頁訂閱清單的 HostSubscriptionIndexViewModel 的 IActionResult</returns>
		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] HostSubscriptionSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
		{
			// 預設排序
			if (string.IsNullOrWhiteSpace(criteria.SortBy))
				criteria.SortBy = "CreatedAt";

			if (!Request.Query.ContainsKey(nameof(criteria.IsDescending)))
				criteria.IsDescending = true;

			// 判斷：有任一篩選條件或排序參數就走搜尋管線
			bool hasFilter = HasAnyFilter(criteria);
			bool hasSort = !string.IsNullOrWhiteSpace(criteria.SortBy)
						   || Request.Query.ContainsKey(nameof(criteria.IsDescending));

			var pagedResult = (hasFilter || hasSort)
				? await _hostSubscriptionService.SearchHostSubscriptionsAsync(criteria, pageIndex, pageSize)
				: await _hostSubscriptionService.GetPagedHostSubscriptionsAsync(pageIndex, pageSize);

			var vm = new HostSubscriptionIndexViewModel
			{
				HostSubscriptions = pagedResult.Items.Select(hs => new HostSubscriptionIndexRowViewModel
				{
					HostSubId = hs.HostSubId,
					HostId = hs.HostId,
					HostName = hs.HostName,
					PlanName = hs.PlanName,
					StartDate = hs.StartDate,
					NextBillingDate = hs.NextBillingDate,
					CancelAtPeriodEnd = hs.CancelAtPeriodEnd,
					Status = hs.Status,
					CreatedAt = hs.CreatedAt
				}).ToList(),

				PageIndex = pagedResult.PageIndex,
				TotalPages = pagedResult.TotalPages,
				PageSize = pagedResult.PageSize,
				TotalCount = pagedResult.TotalCount,

				// 將搜尋條件帶回 View 供回填用
				Criteria = criteria
			};

			ViewData["ActiveTab"] = "host";
			return View(vm);
		}

		[HttpGet]
		public async Task<IActionResult> Details(int id)
		{
			var hostSubscriptionDto = await _hostSubscriptionService.GetHostSubscriptionByIdAsync(id);

			if (hostSubscriptionDto == null)
			{
				return NotFound();
			}

			// 手動映射以排除 AutoMapper 問題
			var vm = new HostSubscriptionDetailViewModel
			{
				HostSubId = hostSubscriptionDto.HostSubId,
				HostId = hostSubscriptionDto.HostId,
				PlanId = hostSubscriptionDto.PlanId,
				StartDate = hostSubscriptionDto.StartDate,
				NextBillingDate = hostSubscriptionDto.NextBillingDate,
				CancelAtPeriodEnd = hostSubscriptionDto.CancelAtPeriodEnd,
				Status = hostSubscriptionDto.Status,
				CreatedAt = hostSubscriptionDto.CreatedAt,
				HostName = hostSubscriptionDto.HostName,
				PlanName = hostSubscriptionDto.PlanName,
				Billings = hostSubscriptionDto.Billings?.Select(b => new HostSubscriptionBillingViewModel
				{
					BillId = b.BillId,
					HostSubId = b.HostSubId,
					Amount = b.Amount,
					PaidStatus = b.PaidStatus,
					PaidAt = b.PaidAt,
					CreatedAt = b.CreatedAt,
					Note = b.Note
				}).ToList() ?? new List<HostSubscriptionBillingViewModel>()
			};

			// 如果是 AJAX 請求，返回部分檢視
			if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
			{
				return PartialView("_HostSubscriptionDetailPartial", vm);
			}

			ViewData["ActiveTab"] = "host";
			return View(vm);
		}

		// 匯出 Excel
		[HttpGet]
		public async Task<IActionResult> ExportExcel()
		{
			var result = await _hostSubscriptionService.GetPagedHostSubscriptionsAsync(1, int.MaxValue);

			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("訂閱管理列表");

			// 標題列
			worksheet.Cell(1, 1).Value = "訂閱編號";
			worksheet.Cell(1, 2).Value = "房東姓名";
			worksheet.Cell(1, 3).Value = "方案名稱";
			worksheet.Cell(1, 4).Value = "開始日期";
			worksheet.Cell(1, 5).Value = "下次計費日期";
			worksheet.Cell(1, 6).Value = "期末取消";
			worksheet.Cell(1, 7).Value = "狀態";
			worksheet.Cell(1, 8).Value = "建立時間";

			// 資料列
			int row = 2;
			foreach (var item in result.Items)
			{
				worksheet.Cell(row, 1).Value = item.HostSubId;
				worksheet.Cell(row, 2).Value = item.HostName ?? "-";
				worksheet.Cell(row, 3).Value = item.PlanName ?? "-";
				worksheet.Cell(row, 4).Value = item.StartDate?.ToString("yyyy-MM-dd") ?? "-";
				worksheet.Cell(row, 5).Value = item.NextBillingDate?.ToString("yyyy-MM-dd") ?? "-";
				worksheet.Cell(row, 6).Value = item.CancelAtPeriodEnd == true ? "是" : "否";
				worksheet.Cell(row, 7).Value = item.Status switch
				{
					"active" => "啟用中",
					"canceled" => "已取消",
					"expired" => "已到期",
					_ => item.Status ?? "未知"
				};
				worksheet.Cell(row, 8).Value = item.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-";
				row++;
			}

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Seek(0, SeekOrigin.Begin);

			return File(stream.ToArray(),
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				"訂閱管理列表.xlsx");
		}

		// 匯出 CSV
		[HttpGet]
		public async Task<IActionResult> ExportCsv()
		{
			var result = await _hostSubscriptionService.GetPagedHostSubscriptionsAsync(1, int.MaxValue);

			var sb = new StringBuilder();
			sb.AppendLine("訂閱編號,房東姓名,方案名稱,開始日期,下次計費日期,期末取消,狀態,建立時間");

			foreach (var item in result.Items)
			{
				var status = item.Status switch
				{
					"active" => "啟用中",
					"canceled" => "已取消",
					"expired" => "已到期",
					_ => item.Status ?? "未知"
				};

				sb.AppendLine($"{item.HostSubId}," +
							  $"{item.HostName ?? "-"}," +
							  $"{item.PlanName ?? "-"}," +
							  $"{item.StartDate?.ToString("yyyy-MM-dd") ?? "-"}," +
							  $"{item.NextBillingDate?.ToString("yyyy-MM-dd") ?? "-"}," +
							  $"{(item.CancelAtPeriodEnd == true ? "是" : "否")}," +
							  $"{status}," +
							  $"{item.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss") ?? "-"}");
			}

			return File(Encoding.UTF8.GetBytes(sb.ToString()),
				"text/csv",
				"訂閱管理列表.csv");
		}

		// ---- Private Helpers ----

		/// <summary>
		/// 檢查搜尋條件物件是否包含任何有效的篩選條件
		/// </summary>
		private static bool HasAnyFilter(HostSubscriptionSearchCriteriaDto criteria)
		{
			if (criteria.HostId.HasValue || criteria.PlanId.HasValue) return true;
			if (!string.IsNullOrWhiteSpace(criteria.Status)) return true;
			if (!string.IsNullOrWhiteSpace(criteria.HostName)) return true;
			if (!string.IsNullOrWhiteSpace(criteria.PlanName)) return true;

			// 日期範圍
			if (criteria.StartDateFrom.HasValue || criteria.StartDateTo.HasValue) return true;
			if (criteria.CreatedAtFrom.HasValue || criteria.CreatedAtTo.HasValue) return true;
			if (criteria.NextBillingDateFrom.HasValue || criteria.NextBillingDateTo.HasValue) return true;

			return false;
		}
	}
}
