using AutoMapper;
using ClosedXML.Excel;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.PointRules.ViewModels;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Services.PointRules;
using System.Text;

namespace RentalManagementPlatformMVC.Areas.PointRules.Controllers
{
    [Area("PointRules")]
    public class PointLedgerHomeController : Controller
    {
        private readonly IPointLedgerService _pointLedgerService;
        private readonly IMapper _mapper;

        public PointLedgerHomeController(IPointLedgerService pointLedgerService, IMapper mapper)
        {
            _pointLedgerService = pointLedgerService;
            _mapper = mapper;
        }

        /// <summary>
        /// 點數帳本管理主頁面，根據是否有搜尋條件決定顯示初始清單或搜尋結果。
        /// 當有任何篩選條件或排序參數時，會執行搜尋功能；否則顯示預設的分頁清單。
        /// </summary>
        /// <param name="criteria">搜尋條件物件，包含客戶姓名、點數類型、點數範圍、日期區間及排序參數</param>
        /// <param name="pageIndex">目前頁碼，預設為第1頁</param>
        /// <param name="pageSize">每頁顯示筆數，預設為20筆</param>
        /// <returns>返回包含分頁點數帳本清單的 PointLedgerIndexViewModel 的 IActionResult</returns>
        [HttpGet]
        public async Task<IActionResult> Index([FromQuery] PointLedgerSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
        {
            // 預設排序
            if (string.IsNullOrWhiteSpace(criteria.SortBy))
                criteria.SortBy = "OccurredAt";

            if (!Request.Query.ContainsKey(nameof(criteria.IsDescending)))
                criteria.IsDescending = true;

            // 判斷：有任一篩選條件或排序參數就走搜尋管線
            bool hasFilter = HasAnyFilter(criteria);
            bool hasSort = !string.IsNullOrWhiteSpace(criteria.SortBy)
                           || Request.Query.ContainsKey(nameof(criteria.IsDescending));

            var pagedResult = (hasFilter || hasSort)
                ? await _pointLedgerService.SearchPointLedgersAsync(criteria, pageIndex, pageSize)
                : await _pointLedgerService.GetPagedPointLedgersAsync(pageIndex, pageSize);

            var vm = new PointLedgerIndexViewModel
            {
                PointLedgers = _mapper.Map<List<PointLedgerIndexRowViewModel>>(pagedResult.Items),
                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages,
                TotalCount = pagedResult.TotalCount,

                // 將搜尋條件帶回 View 供回填用
                Criteria = criteria
            };

            ViewData["ActiveTab"] = "ledger";
            return View(vm);
        }

        // ---- Private Helpers ----

        /// <summary>
        /// 檢查搜尋條件物件是否包含任何有效的篩選條件
        /// </summary>
        private static bool HasAnyFilter(PointLedgerSearchCriteriaDto criteria)
        {
            if (criteria.GuestId.HasValue) return true;
            if (!string.IsNullOrWhiteSpace(criteria.GuestName)) return true;
            if (!string.IsNullOrWhiteSpace(criteria.OrderNumberSnapshot)) return true;
            if (!string.IsNullOrWhiteSpace(criteria.Type)) return true;
            if (!string.IsNullOrWhiteSpace(criteria.Note)) return true;
            if (criteria.IsExpired.HasValue) return true;

            // 數值範圍
            if (criteria.PointsFrom.HasValue || criteria.PointsTo.HasValue) return true;

            // 日期範圍
            if (criteria.OccurredAtFrom.HasValue || criteria.OccurredAtTo.HasValue) return true;
            if (criteria.ExpiresAtFrom.HasValue || criteria.ExpiresAtTo.HasValue) return true;

            return false;
        }

        // 匯出 Excel
        [HttpGet]
        public async Task<IActionResult> ExportExcel([FromQuery] PointLedgerSearchCriteriaDto criteria)
        {
            // 使用搜尋功能或預設查詢取得所有資料
            bool hasFilter = HasAnyFilter(criteria);
            var result = hasFilter
                ? await _pointLedgerService.SearchPointLedgersAsync(criteria, 1, int.MaxValue)
                : await _pointLedgerService.GetPagedPointLedgersAsync(1, int.MaxValue);

            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("點數帳本列表");

            // 標題列
            worksheet.Cell(1, 2).Value = "房客姓名";
            worksheet.Cell(1, 3).Value = "訂單號碼";
            worksheet.Cell(1, 4).Value = "點數類型";
            worksheet.Cell(1, 5).Value = "點數";
            worksheet.Cell(1, 6).Value = "發生時間";
            worksheet.Cell(1, 7).Value = "到期時間";
            worksheet.Cell(1, 9).Value = "備註";

            // 資料列
            int row = 2;
            foreach (var item in result.Items)
            {
                worksheet.Cell(row, 1).Value = item.LedgerId;
                worksheet.Cell(row, 2).Value = item.GuestName ?? "-";
                worksheet.Cell(row, 3).Value = item.OrderNumberSnapshot ?? "-";
                worksheet.Cell(row, 4).Value = item.TypeDisplay;
                worksheet.Cell(row, 5).Value = item.Points;
                worksheet.Cell(row, 6).Value = item.OccurredAt?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cell(row, 7).Value = item.ExpiresAt?.ToString("yyyy-MM-dd") ?? "-";
                worksheet.Cell(row, 8).Value = item.Note ?? "-";
                row++;
            }

            using var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Seek(0, SeekOrigin.Begin);

            return File(stream.ToArray(),
                "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                "點數帳本列表.xlsx");
        }

        // 匯出 CSV
        [HttpGet]
        public async Task<IActionResult> ExportCsv([FromQuery] PointLedgerSearchCriteriaDto criteria)
        {
            // 使用搜尋功能或預設查詢取得所有資料
            bool hasFilter = HasAnyFilter(criteria);
            var result = hasFilter
                ? await _pointLedgerService.SearchPointLedgersAsync(criteria, 1, int.MaxValue)
                : await _pointLedgerService.GetPagedPointLedgersAsync(1, int.MaxValue);

            var sb = new StringBuilder();
            sb.AppendLine("房客姓名,訂單號碼,點數類型,點數,發生時間,到期時間,備註");

            foreach (var item in result.Items)
            {
                sb.AppendLine($"{item.GuestName ?? "-"}," +
                              $"{item.OrderNumberSnapshot ?? "-"}," +
                              $"{item.TypeDisplay}," +
                              $"{item.Points}," +
                              $"{item.OccurredAt?.ToString("yyyy-MM-dd") ?? "-"}," +
                              $"{item.ExpiresAt?.ToString("yyyy-MM-dd") ?? "-"}," +
                              $"{item.Note ?? "-"}");
            }

            return File(Encoding.UTF8.GetBytes(sb.ToString()),
                "text/csv",
                "點數帳本列表.csv");
        }
    }
}