using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
    public class BookingRepository : IBookingRepository
    {
        private readonly BookingDbContext _context;

        public BookingRepository(BookingDbContext context)
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
                .Include(b => b.BookingGuests)
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
					GuestName = b.BookingGuests
							 .Select(g => g.GuestName)
							 .FirstOrDefault(),
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

        public async Task<BookingDetailDto> GetBookingDetailByIdAsync(int bookingId)
        {
            var booking = await _context.Bookings
				.AsNoTracking()
				.Include(b => b.BookingGuests)
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
                CouponId = booking.CouponId,
                GuestId = booking.GuestId,
                RoomId = booking.RoomId,
                Guests = booking.BookingGuests?.Select(g => new BookingGuestDto
                {
                    GuestName = g.GuestName,
                })
                .ToList() ?? new List<BookingGuestDto>()
            };

            return dto;
        }
    }
}
