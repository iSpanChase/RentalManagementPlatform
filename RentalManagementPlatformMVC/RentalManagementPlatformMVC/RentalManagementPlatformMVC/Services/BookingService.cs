using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Repositories;

namespace RentalManagementPlatformMVC.Services
{
    public class BookingService : IBookingService
    {
        private readonly IBookingRepository _bookingRepository;

        public BookingService(IBookingRepository bookingRepository)
        {
            _bookingRepository = bookingRepository;
        }

		/// <summary>
		/// 初始載入：取得所有訂單資料（支援分頁）
		/// </summary>
		public async Task<PagedResultDto<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize)
        {
			var (entities, totalCount) = await _bookingRepository.GetPagedBookingsAsync(pageIndex, pageSize);

			var bookingDto = entities.Select(b => new BookingDto
			{
				BookingId = b.BookingId,
				OrderNumber = b.OrderNumber,
				CheckIn = b.CheckIn,
				CheckOut = b.CheckOut,
				TotalPrice = b.TotalPrice,
				Status = b.Status,
				CreatedAt = b.CreatedAt,
				GuestName = b.Guest?.Name
			}).ToList();

			return new PagedResultDto<BookingDto>
			{
				Items = bookingDto,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 詳細頁面：根據訂單ID取得訂單詳細資訊
		/// </summary>
		public async Task<BookingDetailDto?> GetBookingDetailByIdAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetByIdAsync(bookingId);

			if (booking is null) return null;

			return new BookingDetailDto
			{
				BookingId = booking.BookingId,
				OrderNumber = booking.OrderNumber,
				CheckIn = booking.CheckIn,
				CheckOut = booking.CheckOut,
				TotalPrice = booking.TotalPrice,
				CommissionRateSnapshot = booking.CommissionRateSnapshot,
				PointsEarned = booking.PointsEarned,
				PointsRedeemed = booking.PointsRedeemed,
				Status = booking.Status,
				CreatedAt = booking.CreatedAt,
				GuestName = booking.Guest?.Name,                  // 安全取值
				Coupon = booking.Coupon?.CouponName,              // 安全取值
				Room = booking.Room?.Title,                       // 安全取值
				Guests = booking.BookingGuests?
					.Select(g => new BookingGuestDto { GuestName = g.GuestName })
					.ToList() ?? new List<BookingGuestDto>()      // 永遠給不為 null 的集合
			};
		}

		/// <summary>
		/// 條件查詢：根據篩選條件查詢訂單
		/// </summary>
		public async Task<PagedResultDto<BookingDto>> SearchBookingsAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 可選：簡單規格修正（避免使用者傳錯）
			if (criteria.MinPrice.HasValue && criteria.MaxPrice.HasValue &&
				criteria.MinPrice > criteria.MaxPrice)
			{
				// 交換，或丟出驗證錯誤都行
				(criteria.MinPrice, criteria.MaxPrice) = (criteria.MaxPrice, criteria.MinPrice);
			}

			var (entities, total) = await _bookingRepository.SearchAsync(criteria, pageIndex, pageSize);

			var items = entities.Select(b => new BookingDto
			{
				BookingId = b.BookingId,
				OrderNumber = b.OrderNumber,
				GuestName = b.Guest?.Name,
				CheckIn = b.CheckIn,
				CheckOut = b.CheckOut,
				TotalPrice = b.TotalPrice,
				Status = b.Status,
				CreatedAt = b.CreatedAt,
				GuestCount = b.BookingGuests?.Count ?? 0,
				Room = b.Room?.Title
			}).ToList();

			return new PagedResultDto<BookingDto>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = total
			};
		}
    }
}