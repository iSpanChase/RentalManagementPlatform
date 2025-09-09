using ClosedXML.Excel;
using Humanizer;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Areas.Payments.ViewModels;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Services;
using System.Text;

namespace RentalManagementPlatformMVC.Areas.Payments.Controllers
{
	[Area("Payments")]
	public class PaymentsHomeController : Controller
	{
		private readonly IPaymentService _paymentService;

		public PaymentsHomeController(IPaymentService paymentService)
		{
			_paymentService = paymentService;
		}

		/// <summary>
		/// 付款首頁控制器，負責處理付款相關的列表顯示、搜尋、詳細資料查看及匯出功能
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] PaymentSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
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
				? await _paymentService.SearchPaymentsAsync(criteria, pageIndex, pageSize)
				: await _paymentService.GetPagedPaymentsAsync(pageIndex, pageSize);

			// 回填 ViewModel
			var vm = new PaymentIndexViewModel
			{
				Payments = paged.Items.Select(p => new PaymentIndexRowViewModel
				{
					PaymentId = p.PaymentId,
					OrderNumberSnapshot = p.OrderNumberSnapshot,
					Amount = p.Amount,
					PaymentRef = p.PaymentRef,
					Method = p.Method,
					PaidAt = p.PaidAt,
					Status = p.Status,
					CreatedAt = p.CreatedAt,
				}).ToList(),

				PageIndex = paged.PageIndex,
				PageSize = paged.PageSize,
				TotalPages = paged.TotalPages,
				TotalCount = paged.TotalCount,

				Criteria = criteria
			};

			return View(vm);
		}

		/// <summary>
		/// 取得指定付款的詳細資訊，用於顯示付款詳細資料彈窗
		/// </summary>
		/// <param name="paymentId">付款編號</param>
		/// <returns>返回付款詳細資料的部分檢視，若找不到則返回NotFound</returns>
		[HttpGet]
		public async Task<IActionResult> Details(int paymentId)
		{
			var paymentDetail = await _paymentService.GetPaymentDetailByIdAsync(paymentId);

			if (paymentDetail == null) return NotFound();

			var vm = new PaymentDetailViewModel
			{
				PaymentId = paymentDetail.PaymentId,
				BookingId = paymentDetail.BookingId,
				Amount = paymentDetail.Amount,
				Method = paymentDetail.Method,
				PaidAt = paymentDetail.PaidAt,
				PaymentRef = paymentDetail.PaymentRef,
				OrderNumberSnapshot = paymentDetail.OrderNumberSnapshot,
				Status = paymentDetail.Status,
				PaymentCreatedAt = paymentDetail.PaymentCreatedAt,
				GuestName = paymentDetail.GuestName,
				RoomTitle = paymentDetail.RoomTitle,
				Transactions = paymentDetail.Transactions?
								   .OrderByDescending(t => t.TransactionCreatedAt)
								   .ToList() ?? new()
			};

			return PartialView("_PaymentDetailPartial", vm);
		}

		/// <summary>
		/// 檢查付款搜尋條件是否包含任何篩選條件
		/// </summary>
		/// <param name="c">付款搜尋條件物件</param>
		/// <returns>如果包含任何篩選條件則返回 true，否則返回 false</returns>
		private static bool HasAnyFilter(PaymentSearchCriteriaDto c)
		{
			if (c == null) return false;

			// 文字/識別條件
			if (!string.IsNullOrWhiteSpace(c.OrderNumber)) return true;
			if (!string.IsNullOrWhiteSpace(c.PaymentRef)) return true;
			if (!string.IsNullOrWhiteSpace(c.TransactionRef)) return true;
			if (!string.IsNullOrWhiteSpace(c.Status)) return true;
			if (!string.IsNullOrWhiteSpace(c.GuestName)) return true;
			if (!string.IsNullOrWhiteSpace(c.RoomTitle)) return true;

			// 日期區間（付款時間）
			if (c.PaidStartDate.HasValue || c.PaidEndDate.HasValue) return true;

			// 金額區間
			if (c.MinAmount.HasValue || c.MaxAmount.HasValue) return true;

			// 排序不算篩選
			return false;
		}

		// 匯出 Excel
		[HttpGet]
		public async Task<IActionResult> ExportExcel([FromQuery] PaymentSearchCriteriaDto criteria)
		{
			var result = await _paymentService.SearchPaymentsAsync(criteria, 1, int.MaxValue);

			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("付款列表");

			// 標題列
			worksheet.Cell(1, 1).Value = "訂單編號";
			worksheet.Cell(1, 2).Value = "金額";
			worksheet.Cell(1, 3).Value = "平台付款編號 (PaymentRef)";
			worksheet.Cell(1, 4).Value = "付款方式";
			worksheet.Cell(1, 5).Value = "付款時間";
			worksheet.Cell(1, 6).Value = "狀態";
			worksheet.Cell(1, 7).Value = "建立時間";

			// 資料列
			int row = 2;
			foreach (var p in result.Items)
			{
				worksheet.Cell(row, 1).Value = p.OrderNumberSnapshot;
				worksheet.Cell(row, 2).Value = p.Amount;
				worksheet.Cell(row, 3).Value = p.PaymentRef;
				worksheet.Cell(row, 4).Value = p.Method;
				worksheet.Cell(row, 5).Value = p.PaidAt?.ToString("yyyy-MM-dd HH:mm");
				worksheet.Cell(row, 6).Value = p.Status;
				worksheet.Cell(row, 7).Value = p.CreatedAt?.ToString("yyyy-MM-dd HH:mm");
				row++;
			}

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Seek(0, SeekOrigin.Begin);

			return File(stream.ToArray(),
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				"付款列表.xlsx");
		}

		// 匯出 CSV
		[HttpGet]
		public async Task<IActionResult> ExportCsv([FromQuery] PaymentSearchCriteriaDto criteria)
		{
			var result = await _paymentService.SearchPaymentsAsync(criteria, 1, int.MaxValue);

			var sb = new StringBuilder();
			sb.AppendLine("訂單編號,金額,平台付款編號( PaymentRef ),付款方式,付款時間,狀態,建立時間");

			foreach (var p in result.Items)
			{
				// 簡單 CSV：若資料可能含逗號/換行，建議加上引號轉義
				sb.AppendLine($"{p.OrderNumberSnapshot},{p.Amount},{p.PaymentRef},{p.Method},{p.PaidAt:yyyy-MM-dd HH:mm},{p.Status},{p.CreatedAt:yyyy-MM-dd HH:mm}");
			}

			return File(Encoding.UTF8.GetBytes(sb.ToString()),
				"text/csv",
				"付款列表.csv");
		}
	}
}