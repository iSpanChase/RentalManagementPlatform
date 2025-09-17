using ClosedXML.Excel;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Payments.ViewModels;
using RentalManagementPlatformMVC.DTOs.Payments;
using RentalManagementPlatformMVC.Services.Payments;
using System.Text;

namespace RentalManagementPlatformMVC.Areas.Payments.Controllers
{
	[Area("Payments")]
	[Authorize]
	public class HostPayoutHomeController : Controller
	{
		private readonly IHostPayoutService _hostPayoutService;

		public HostPayoutHomeController(IHostPayoutService hostPayoutService)
		{
			_hostPayoutService = hostPayoutService;
		}

		/// <summary>
		/// 顯示房東撥款清單頁面，支援條件篩選、排序和分頁功能
		/// </summary>
		/// <param name="criteria">搜尋條件，包含房東資訊、週期日期、付款日期、金額範圍、狀態等篩選條件</param>
		/// <param name="pageIndex">目前頁碼，預設為第1頁</param>
		/// <param name="pageSize">每頁顯示筆數，預設為20筆</param>
		/// <returns>包含房東撥款清單資料的視圖結果，若有篩選條件則回傳搜尋結果，否則回傳一般分頁資料</returns>
		[HttpGet]
		public async Task<IActionResult> Index(HostSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
		{
			// 預設排序（首次載入或沒帶時）
			if (string.IsNullOrWhiteSpace(criteria?.SortBy))
				criteria!.SortBy = "CreatedAt";

			// 若沒帶 IsDescending 這個 query key，預設為 true（desc）
			if (!Request.Query.ContainsKey(nameof(criteria.IsDescending)))
				criteria.IsDescending = true;

			// 有任一篩選「或有排序參數」就走搜尋管線
			bool hasFilter = HasAnyFilter(criteria);
			bool hasSort = !string.IsNullOrWhiteSpace(criteria.SortBy)
				|| Request.Query.ContainsKey(nameof(criteria.IsDescending));

			var paged = (hasFilter || hasSort)
				? await _hostPayoutService.SearchHostPayoutsAsync(criteria, pageIndex, pageSize)
				: await _hostPayoutService.GetPagedHostPayoutsAsync(pageIndex, pageSize);

			// 回填 ViewModel

			var vm = new HostPayoutIndexViewModel
			{
				HostPayouts = paged.Items.Select(p => new HostPayoutIndexRowViewModel
				{
					PayoutId = p.PayoutId,
					HostId = p.HostId,
					HostName = p.HostName,
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

				Criteria = criteria,
			};

			ViewData["ActiveTab"] = "host";
			return View(vm);
		}

		/// <summary>
		/// 取得指定房東撥款的詳細資訊，包含撥款基本資料和關聯的撥款項目明細
		/// </summary>
		/// <param name="hostPayoutId">房東撥款的唯一識別碼</param>
		/// <returns>若找到對應的撥款資料則回傳包含詳細資訊的部分視圖，若找不到則回傳 NotFound 結果</returns>
		[HttpGet]
		public async Task<IActionResult> Details(int hostPayoutId)
		{
			var hostPayout = await _hostPayoutService.GetHostPayoutByIdAsync(hostPayoutId);
			if (hostPayout == null) return NotFound();

			var vm = new HostPayoutDetailViewModel
			{
				PayoutId = hostPayout.PayoutId,
				HostName = hostPayout?.HostName,
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

		/// <summary>
		/// 檢查搜尋條件是否包含任何篩選條件，用於判斷是否需要執行篩選查詢
		/// </summary>
		/// <param name="c">房東撥款搜尋條件物件，包含各種篩選參數如房東資訊、週期日期、付款日期、金額範圍和狀態等</param>
		/// <returns>若搜尋條件中包含任何有效的篩選條件則回傳 true，否則回傳 false。排序條件不算在篩選條件內</returns>
		private static bool HasAnyFilter(HostSearchCriteriaDto c)
		{
			if (c == null) return false;

			// Host識別條件
			if (c.HostId.HasValue) return true;
			if (!string.IsNullOrWhiteSpace(c.HostName)) return true;
			if (!string.IsNullOrWhiteSpace(c.Status)) return true;

			// 撥款週期區間
			if (c.CycleStartDate.HasValue || c.CycleEndDate.HasValue) return true;

			// 付款日期區間
			if (c.PaidStartDate.HasValue || c.PaidEndDate.HasValue) return true;

			// 金額區間
			if (c.MinAmount.HasValue || c.MaxAmount.HasValue) return true;

			// 排序不算篩選
			return false;
		}

		// 匯出 Excel
		[HttpGet]
		public async Task<IActionResult> ExportExcel([FromQuery] HostSearchCriteriaDto criteria)
		{
			var result = await _hostPayoutService.SearchHostPayoutsAsync(criteria, 1, int.MaxValue);

			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("房東撥款列表");

			// 標題列
			worksheet.Cell(1, 1).Value = "房東姓名";
			worksheet.Cell(1, 2).Value = "週期起始";
			worksheet.Cell(1, 3).Value = "週期結束";
			worksheet.Cell(1, 4).Value = "匯款時間";
			worksheet.Cell(1, 5).Value = "狀態";
			worksheet.Cell(1, 6).Value = "建立時間";

			// 資料列
			int row = 2;
			foreach (var p in result.Items)
			{
				worksheet.Cell(row, 1).Value = p.HostName;
				worksheet.Cell(row, 2).Value = p.CycleStart?.ToString("yyyy-MM-dd");
				worksheet.Cell(row, 3).Value = p.CycleEnd?.ToString("yyyy-MM-dd");
				worksheet.Cell(row, 4).Value = p.PaidAt?.ToString("yyyy-MM-dd HH:mm");
				worksheet.Cell(row, 5).Value = p.Status;
				worksheet.Cell(row, 6).Value = p.CreatedAt?.ToString("yyyy-MM-dd HH:mm");
				row++;
			}

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Seek(0, SeekOrigin.Begin);

			return File(stream.ToArray(),
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				"房東撥款列表.xlsx");
		}

		// 匯出 CSV
		[HttpGet]
		public async Task<IActionResult> ExportCsv([FromQuery] HostSearchCriteriaDto criteria)
		{
			var result = await _hostPayoutService.SearchHostPayoutsAsync(criteria, 1, int.MaxValue);

			var sb = new StringBuilder();
			sb.AppendLine("房東姓名,週期起始,週期結束,匯款時間,狀態,建立時間");

			foreach (var p in result.Items)
			{
				// 簡單 CSV：若資料可能含逗號/換行，建議加上引號轉義
				sb.AppendLine($"{p.HostName},{p.CycleStart:yyyy-MM-dd},{p.CycleEnd:yyyy-MM-dd},{p.PaidAt:yyyy-MM-dd HH:mm},{p.Status},{p.CreatedAt:yyyy-MM-dd HH:mm}");
			}

			return File(Encoding.UTF8.GetBytes(sb.ToString()),
				"text/csv",
				"房東撥款列表.csv");
		}
	}
}
