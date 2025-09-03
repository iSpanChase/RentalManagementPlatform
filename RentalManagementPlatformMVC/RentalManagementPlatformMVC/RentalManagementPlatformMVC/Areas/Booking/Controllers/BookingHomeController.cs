using Microsoft.AspNetCore.Mvc;
using RentalManagementPlatformMVC.Services;
using RentalManagementPlatformMVC.Areas.Booking.ViewModels;

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
		/// 顯示訂單管理頁面（含分頁功能）。
		/// </summary>
		/// <param name="pageIndex">目前頁碼（預設為 1）。</param>
		/// <param name="pageSize">每頁顯示筆數（預設為 20）。</param>
		/// <returns>回傳包含訂單清單與分頁資訊的 View。</returns>
		public async Task<IActionResult> Index(int pageIndex = 1, int pageSize = 20)
        {
            var pagedResult = await _bookingService.GetPagedBookingsAsync(pageIndex, pageSize);

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
                    CreatedAt = b.CreatedAt,
                })
                .ToList(),

                PageIndex = pagedResult.PageIndex,
                PageSize = pagedResult.PageSize,
                TotalPages = pagedResult.TotalPages,
                TotalCount = pagedResult.TotalCount
			};

            return View(vm);
        }

        /// <summary>
        ///     
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
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
                CouponId = dto.CouponId,
                GuestId = dto.GuestId,
                RoomId = dto.RoomId,
                Guests = dto.Guests
            };
            return PartialView("_BookingDetailModal", vm);
        }
    }
}
