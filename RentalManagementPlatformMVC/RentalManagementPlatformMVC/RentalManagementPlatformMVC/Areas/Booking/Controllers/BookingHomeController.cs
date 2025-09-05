using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Areas.Booking.ViewModels;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Services;

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
		/// 訂單管理（有帶任一條件→搜尋；否則→初始清單）
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Index([FromQuery] BookingSearchCriteriaDto criteria, int pageIndex = 1, int pageSize = 20)
		{
			// 1) 預設排序（首次載入或沒帶時）
			if (string.IsNullOrWhiteSpace(criteria.SortBy))
				criteria.SortBy = "createdAt";

			// 若沒帶 IsDescending 這個 query key，預設為 true（desc）
			if (!Request.Query.ContainsKey(nameof(criteria.IsDescending)))
				criteria.IsDescending = true;

			// 2) 判斷：有任一篩選「或有排序參數」就走搜尋管線
			bool hasFilter = HasAnyFilter(criteria);
			bool hasSort = !string.IsNullOrWhiteSpace(criteria.SortBy)
						   || Request.Query.ContainsKey(nameof(criteria.IsDescending));

			var pagedResult = (hasFilter || hasSort)
				? await _bookingService.SearchBookingsAsync(criteria, pageIndex, pageSize)
				: await _bookingService.GetPagedBookingsAsync(pageIndex, pageSize);

			// 3) 用 ViewModel 帶回 criteria（不要只放 ViewBag）
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

				// << 關鍵：給 View 回填用
				Criteria = criteria
			};

			return View(vm);
		}

		/// <summary>
		/// 訂單詳細（Modal）
		/// </summary>
		[HttpGet]
		public async Task<IActionResult> Detail(int id)
		{
			var dto = await _bookingService.GetBookingDetailByIdAsync(id);
			if (dto == null) return NotFound();

			var vm = new BookingDetailViewModel
			{
				BookingId = dto.BookingId,
				OrderNumber = dto.OrderNumber,
				CheckIn = dto.CheckIn,
				CheckOut = dto.CheckOut,
				TotalPrice = dto.TotalPrice,
				CommissionRateSnapshot = dto.CommissionRateSnapshot,
				PointsEarned = dto.PointsEarned,
				PointsRedeemed = dto.PointsRedeemed,
				Status = dto.Status,
				CreatedAt = dto.CreatedAt,
				GuestName = dto.GuestName,
				Coupon = dto.Coupon,
				Room = dto.Room,
				Guests = dto.Guests
			};

			return PartialView("_BookingDetailModal", vm);
		}

		[HttpGet]
		public IActionResult Search() => View();

		// ---- Private Helpers ----
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
	}
}
