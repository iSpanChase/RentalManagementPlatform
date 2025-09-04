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
		public async Task<IActionResult> Index([FromQuery] BookingSearchCriteria criteria, int pageIndex = 1, int pageSize = 20)
		{
			// 判斷是否有任一條件被填寫
			bool hasFilter = HasAnyFilter(criteria);

			var pagedResult = hasFilter
				? await _bookingService.SearchBookingsAsync(criteria, pageIndex, pageSize)
				: await _bookingService.GetPagedBookingsAsync(pageIndex, pageSize);

			// 暫存條件到 ViewBag（之後建議改成 ViewModel.Criteria）
			ViewBag.Criteria = criteria;
			ViewBag.HasFilter = hasFilter;

			var vm = new BookingIndexViewModel
			{
				Bookings = pagedResult.Items.Select(b => new BookingIndexRowViewModel
				{
					BookingId = b.BookingId,
					OrderNumber = b.OrderNumber ?? "-",
					GuestName = b.GuestName ?? "-",
					CheckIn = b.CheckIn,
					CheckOut = b.CheckOut,
					TotalPrice = b.TotalPrice,
					Status = b.Status ?? "-",
					CreatedAt = b.CreatedAt
				}).ToList(),

				PageIndex = pagedResult.PageIndex,
				PageSize = pagedResult.PageSize,
				TotalPages = pagedResult.TotalPages,
				TotalCount = pagedResult.TotalCount
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
		private static bool HasAnyFilter(BookingSearchCriteria c)
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
