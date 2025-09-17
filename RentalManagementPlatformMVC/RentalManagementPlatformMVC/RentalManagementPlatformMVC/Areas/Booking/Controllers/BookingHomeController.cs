using ClosedXML.Excel;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Booking.ViewModels;
using RentalManagementPlatformMVC.DTOs.Bookings;
using RentalManagementPlatformMVC.Services.Bookings;
using System.Text;

namespace RentalManagementPlatformMVC.Areas.Booking.Controllers
{
	[Area("Booking")]
	public class BookingHomeController : Controller
	{
		private readonly IBookingService _bookingService;

		public BookingHomeController(IBookingService bookingService)
		{
			_bookingService = bookingService;
		}

		/// <summary>
		/// 訂單管理主頁面，根據是否有搜尋條件決定顯示初始清單或搜尋結果。
		/// 當有任何篩選條件或排序參數時，會執行搜尋功能；否則顯示預設的分頁清單。
		/// 預設按建立時間降序排列。
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含訂單編號、狀態、客人姓名、房間、日期區間、價格區間及排序參數</param>
		/// <param name="pageIndex">目前頁碼，預設為第1頁</param>
		/// <param name="pageSize">每頁顯示筆數，預設為20筆</param>
		/// <returns>返回包含分頁訂單清單的 BookingIndexViewModel 的 IActionResult</returns>
		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] BookingSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
		{
			// 預設排序（首次載入或沒帶時）
			if (string.IsNullOrWhiteSpace(criteria.SortBy))
				criteria.SortBy = "createdAt";

			// 若沒帶 IsDescending 這個 query key，預設為 true（desc）
			if (!Request.Query.ContainsKey(nameof(criteria.IsDescending)))
				criteria.IsDescending = true;

			// 判斷：有任一篩選「或有排序參數」就走搜尋管線
			bool hasFilter = HasAnyFilter(criteria);
			bool hasSort = !string.IsNullOrWhiteSpace(criteria.SortBy)
						   || Request.Query.ContainsKey(nameof(criteria.IsDescending));

			var pagedResult = (hasFilter || hasSort)
				? await _bookingService.SearchBookingsAsync(criteria, pageIndex, pageSize)
				: await _bookingService.GetPagedBookingsAsync(pageIndex, pageSize);

			// 用 ViewModel 帶回 criteria
			var vm = new BookingIndexViewModel
			{
				Bookings = pagedResult.Items.Select(b => new BookingIndexRowViewModel
				{
					BookingId = b.BookingId,
					OrderNumber = b.OrderNumber,
					GuestName = b.GuestName,
					CheckIn = b.CheckIn,
					CheckOut = b.CheckOut,
					TotalPrice = b.TotalPrice,
					Status = b.Status,
					CreatedAt = b.CreatedAt
				}).ToList(),

				PageIndex = pagedResult.PageIndex,
				PageSize = pagedResult.PageSize,
				TotalPages = pagedResult.TotalPages,
				TotalCount = pagedResult.TotalCount,
				Criteria = criteria
			};

			return View(vm);
		}

		/// <summary>
		/// 訂單詳細（Modal）
		/// </summary>
		/// <param name="bookingId">要查詢的訂單識別碼</param>
		/// <returns>
		/// 返回包含訂單詳細資訊的部分檢視 IActionResult。
		/// 若找不到指定的訂單則返回 NotFound 結果。
		/// 成功時返回 "_BookingDetailPartial" 部分檢視及對應的 BookingDetailViewModel。
		/// </returns>
		[HttpGet]
		public async Task<IActionResult> Detail(int bookingId)
		{
			var bookingDto = await _bookingService.GetBookingDetailByIdAsync(bookingId);
			if (bookingDto == null) return NotFound();

			var vm = new BookingDetailViewModel
			{
				BookingId = bookingDto.BookingId,
				OrderNumber = bookingDto.OrderNumber,
				CheckIn = bookingDto.CheckIn,
				CheckOut = bookingDto.CheckOut,
				TotalPrice = bookingDto.TotalPrice,
				CommissionRateSnapshot = bookingDto.CommissionRateSnapshot,
				PointsEarned = bookingDto.PointsEarned,
				PointsRedeemed = bookingDto.PointsRedeemed,
				Status = bookingDto.Status,
				CreatedAt = bookingDto.CreatedAt,
				GuestName = bookingDto.GuestName,
				Coupon = bookingDto.Coupon,
				Room = bookingDto.Room,
				HostName = bookingDto.HostName,
				Guests = bookingDto.Guests
			};

			return PartialView("_BookingDetailPartial", vm);
		}

		/// <summary>
		/// 檢查搜尋條件物件是否包含任何有效的篩選條件。
		/// 此方法用於判斷是否需要執行搜尋功能，而非預設的分頁顯示。
		/// 排序參數不被視為篩選條件，僅影響資料顯示順序。
		/// </summary>
		/// <param name="c">要檢查的搜尋條件物件，包含訂單編號、狀態、客人姓名、房間、日期區間、價格區間等篩選參數</param>
		/// <returns>
		/// 若搜尋條件物件包含任何有效的篩選條件則返回 true；
		/// 若搜尋條件物件為 null 或所有篩選條件皆為空值則返回 false。
		/// </returns>
		private static bool HasAnyFilter(BookingSearchCriteriaDto c)
		{
			if (c == null) return false;

			// 文字條件
			if (!string.IsNullOrWhiteSpace(c.OrderNumber)) return true;
			if (!string.IsNullOrWhiteSpace(c.Status)) return true;
			if (!string.IsNullOrWhiteSpace(c.GuestName)) return true;
			if (!string.IsNullOrWhiteSpace(c.Room)) return true;

			// 日期區間
			if (c.CheckInStartDate.HasValue || c.CheckInEndDate.HasValue) return true;
			if (c.CheckOutStartDate.HasValue || c.CheckOutEndDate.HasValue) return true;

			// 價格區間
			if (c.MinPrice.HasValue || c.MaxPrice.HasValue) return true;

			// 排序不算篩選（只影響顯示）
			return false;
		}

		// 匯出 Excel
		public async Task<IActionResult> ExportExcel(BookingSearchCriteriaDto criteria)
		{
			var bookings = await _bookingService.SearchBookingsAsync(criteria, 1, int.MaxValue);

			using var workbook = new XLWorkbook();
			var worksheet = workbook.Worksheets.Add("訂單列表");

			// 標題列
			worksheet.Cell(1, 1).Value = "訂單編號";
			worksheet.Cell(1, 2).Value = "訂房姓名";
			worksheet.Cell(1, 3).Value = "入住日期";
			worksheet.Cell(1, 4).Value = "退房日期";
			worksheet.Cell(1, 5).Value = "總金額";
			worksheet.Cell(1, 6).Value = "狀態";
			worksheet.Cell(1, 7).Value = "建立時間";

			// 資料列
			int row = 2;
			foreach (var b in bookings.Items)
			{
				worksheet.Cell(row, 1).Value = b.OrderNumber;
				worksheet.Cell(row, 2).Value = b.GuestName;
				worksheet.Cell(row, 3).Value = b.CheckIn?.ToString("yyyy-MM-dd");
				worksheet.Cell(row, 4).Value = b.CheckOut?.ToString("yyyy-MM-dd");
				worksheet.Cell(row, 5).Value = b.TotalPrice;
				worksheet.Cell(row, 6).Value = b.Status;
				worksheet.Cell(row, 7).Value = b.CreatedAt?.ToString("yyyy-MM-dd HH:mm:ss");
				row++;
			}

			using var stream = new MemoryStream();
			workbook.SaveAs(stream);
			stream.Seek(0, SeekOrigin.Begin);

			return File(stream.ToArray(),
				"application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
				"訂單列表.xlsx");
		}

		// 匯出 CSV
		public async Task<IActionResult> ExportCsv(BookingSearchCriteriaDto criteria)
		{
			var bookings = await _bookingService.SearchBookingsAsync(criteria, 1, int.MaxValue);

			var sb = new StringBuilder();
			sb.AppendLine("訂單編號,訂房姓名,入住日期,退房日期,總金額,狀態,建立時間");

			foreach (var b in bookings.Items)
			{
				sb.AppendLine($"{b.OrderNumber},{b.GuestName},{b.CheckIn:yyyy-MM-dd},{b.CheckOut:yyyy-MM-dd},{b.TotalPrice},{b.Status},{b.CreatedAt}");
			}

			return File(Encoding.UTF8.GetBytes(sb.ToString()),
				"text/csv",
				"訂單列表.csv");
		}
	}
}
