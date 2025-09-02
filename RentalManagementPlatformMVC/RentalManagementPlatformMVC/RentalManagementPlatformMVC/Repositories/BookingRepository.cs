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

        public async Task<PagedResult<BookingDto>> GetAllBookingsAsync(int pageIndex = 1, int pageSize = 20)
        {
            var query = _context.Bookings
                .Include(b => b.BookingGuests)
                .OrderByDescending(b => b.CreatedAt);

            var totalCount = await query.CountAsync();

            var items = await query
                .Skip()
        }
    }
}
