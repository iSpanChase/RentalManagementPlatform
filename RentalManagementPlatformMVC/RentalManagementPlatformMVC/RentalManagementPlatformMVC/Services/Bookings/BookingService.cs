using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.Bookings;
using RentalManagementPlatformMVC.Repositories.Bookings;

namespace RentalManagementPlatformMVC.Services.Bookings
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
		/// <param name="pageIndex">頁面索引（從0開始）</param>
		/// <param name="pageSize">每頁資料筆數</param>
		/// <returns>包含分頁訂單資料的結果物件</returns>
		public async Task<PagedResult<BookingDto>> GetPagedBookingsAsync(int pageIndex, int pageSize)
		{
			var pagedEntities = await _bookingRepository.GetPagedBookingsAsync(pageIndex, pageSize);

			var bookingDtos = pagedEntities.Items.Select(b => new BookingDto
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

			return new PagedResult<BookingDto>
			{
				Items = bookingDtos,
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
		}

		/// <summary>
		/// 詳細頁面：根據訂單ID取得訂單詳細資訊
		/// </summary>
		/// <param name="bookingId">訂單的唯一識別碼</param>
		/// <returns>若找到對應訂單則回傳詳細資訊物件，否則回傳null</returns>
		public async Task<BookingDetailDto?> GetBookingDetailByIdAsync(int bookingId)
		{
			var booking = await _bookingRepository.GetBookingDetailByIdAsync(bookingId);

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
				HostName = booking.Room?.Host?.Name,              // 安全取值
				Guests = booking.BookingGuests?
					.Select(g => new BookingGuestDto { GuestName = g.GuestName })
					.ToList() ?? new List<BookingGuestDto>()      // 永遠給不為 null 的集合
			};
		}

		/// <summary>
		/// 條件查詢：根據篩選條件查詢訂單資料，支援多種搜尋條件組合及分頁功能
		/// </summary>
		/// <param name="criteria">包含多種篩選條件的查詢物件，如訂單編號、狀態、客人姓名、房間、日期區間、價格區間等</param>
		/// <param name="pageIndex">頁面索引（從0開始），用於指定要取得第幾頁的資料</param>
		/// <param name="pageSize">每頁資料筆數，用於控制單頁顯示的訂單數量</param>
		/// <returns>包含符合條件的訂單清單、分頁資訊及總筆數的分頁結果物件</returns>
		public async Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			if (criteria.MinPrice.HasValue && criteria.MaxPrice.HasValue &&
				criteria.MinPrice > criteria.MaxPrice)
			{
				// 交換，或丟出驗證錯誤都行
				(criteria.MinPrice, criteria.MaxPrice) = (criteria.MaxPrice, criteria.MinPrice);
			}

			var (entities, total) = await _bookingRepository.SearchBookingsAsync(criteria, pageIndex, pageSize);

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

			return new PagedResult<BookingDto>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = total
			};
		}
    }
}