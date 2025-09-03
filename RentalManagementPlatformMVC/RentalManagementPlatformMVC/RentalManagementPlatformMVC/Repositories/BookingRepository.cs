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
					MainGuestName = b.BookingGuests
							 .Select(g => g.GuestName)
							 .FirstOrDefault(),
					GuestCount = b.BookingGuests.Count()
				})
                .ToListAsync();

            return new PagedResult<BookingDto>
            {
                Items = items,
                TotalCount = totalCount,
                PageIndex = pageIndex,
                PageSize = pageSize
            };
		}
    }
}
