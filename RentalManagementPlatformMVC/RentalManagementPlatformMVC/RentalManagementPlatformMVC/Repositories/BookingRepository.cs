using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly RentalManagementPlatformSqlContext _context;

        public BookingRepository(RentalManagementPlatformSqlContext context)
        {
            _context = context;
        }

		/// <summary>
		/// 初始載入：取得所有訂單資料（支援分頁）
		/// </summary>
		/// <param name="pageIndex"></param>
		/// <param name="pageSize"></param>
		/// <returns></returns>
		public async Task<PagedResult<BookingDto>> GetAllBookingsAsync(int pageIndex, int pageSize)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(b => b.BookingGuests)
                .Include(b => b.Guest)
				.OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip((pageIndex - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookingDto
                {
                    BookingId = b.BookingId,
                    OrderNumber = b.OrderNumber,
                    CheckIn = b.CheckIn,
                    CheckOut = b.CheckOut,
                    TotalPrice = b.TotalPrice,
                    Status = b.Status,
                    CreatedAt = b.CreatedAt,
					GuestName = b.Guest.Name,
					GuestCount = b.BookingGuests.Count()
				})
                .ToListAsync();

            return new PagedResult<BookingDto>
            {
                Items = items,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
		}

		/// <summary>
		/// 詳細頁面：根據訂單ID取得訂單詳細資訊
		/// </summary>
		/// <param name="bookingId"></param>
		/// <returns></returns>
		public async Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
				.AsNoTracking()
				.Include(b => b.BookingGuests)
                .Include(b => b.Guest)
                .Include(b => b.Coupon)
                .Include(b => b.Room)
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);

            if (booking == null) return null;

            var dto = new BookingDetailDto
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
				GuestName = booking.Guest.Name,
				Coupon = booking.Coupon.CouponName,
                Room = booking.Room.Title,
                Guests = booking.BookingGuests?.Select(g => new BookingGuestDto
                {
                    GuestName = g.GuestName,
                })
                .ToList() ?? new List<BookingGuestDto>()
            };

            return dto;
        }

        /// <summary>
        /// 條件查詢：根據篩選條件查詢訂單
        /// </summary>
        /// <param name="criteria"></param>
        /// <returns></returns>
        public async Task<PagedResult<BookingDto>> SearchBookingsAsync(BookingSearchCriteria criteria, int pageIndex, int pageSize)
        {
            var query = _context.Bookings
                .AsNoTracking()
                .Include(b => b.BookingGuests)
                .Include(b => b.Guest)
                .Include(b => b.Room)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(criteria.OrderNumber))
            {
                query = query.Where(b => b.OrderNumber.Contains(criteria.OrderNumber));
            }

            if (!string.IsNullOrWhiteSpace(criteria.Status))
            {
                query = query.Where(b => b.Status == criteria.Status);

            }

            if (!string.IsNullOrWhiteSpace(criteria.GuestName))
            {
                query = query.Where(b => b.Guest.Name.Contains(criteria.GuestName));
			}

            if (!string.IsNullOrWhiteSpace(criteria.Room))
            {
                query = query.Where(b => b.Room.Title.Contains(criteria.Room));
            }

            if (criteria.CheckInStartDate.HasValue)
            {
                query = query.Where(b => b.CheckIn >= criteria.CheckInStartDate.Value);
            }

            if (criteria.CheckInEndDate.HasValue)
            {
                query = query.Where(b => b.CheckIn <= criteria.CheckInEndDate.Value);
            }

            if (criteria.CheckOutStartDate.HasValue)
            {
                query = query.Where(b => b.CheckOut >= criteria.CheckOutStartDate.Value);
            }

            if (criteria.CheckOutEndDate.HasValue)
            {
                query = query.Where(b => b.CheckOut <= criteria.CheckOutEndDate.Value);
            }

            if (criteria.MinPrice.HasValue)
            {
                query = query.Where(b => b.TotalPrice >= criteria.MinPrice.Value);
            }

            if (criteria.MaxPrice.HasValue)
            {
                query = query.Where(b => b.TotalPrice <= criteria.MaxPrice.Value);
            }

            // 排序
            query = criteria.SortBy?.ToLower() switch
            {
                "checkin" => criteria.IsDescending ? query.OrderByDescending(b => b.CheckIn) : query.OrderBy(b => b.CheckIn),
                "checkout" => criteria.IsDescending ? query.OrderByDescending(b => b.CheckOut) : query.OrderBy(b => b.CheckOut),
                "totalprice" => criteria.IsDescending ? query.OrderByDescending(b => b.TotalPrice) : query.OrderBy(b => b.TotalPrice),
                "status" => criteria.IsDescending ? query.OrderByDescending(b => b.Status) : query.OrderBy(b => b.Status),
                "createdat" or _ => criteria.IsDescending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
            };

            var totalCount = await query.CountAsync();

			var items = await query
		        .Skip((pageIndex - 1) * pageSize)
		        .Take(pageSize)
		        .Select(b => new BookingDto
		        {
			        BookingId = b.BookingId,
			        OrderNumber = b.OrderNumber,
			        GuestName = b.Guest.Name,
			        CheckIn = b.CheckIn,
			        CheckOut = b.CheckOut,
			        TotalPrice = b.TotalPrice,
			        Status = b.Status,
			        CreatedAt = b.CreatedAt,
			        GuestCount = b.BookingGuests.Count(),
			        Room = b.Room.Title
		        })
		        .ToListAsync();

			return new PagedResult<BookingDto>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}
	}
}
