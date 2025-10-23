using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interface;

namespace RentalManagementPlatformWebAPI.Repositories
{
	public class BookingRepository : IBookingRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public BookingRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		// 取得所有訂單(測試用)
		public async Task<IEnumerable<Booking>> GetAllBookingsAsync()
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.Guest)
				.Include(b => b.Room)
				.Include(b => b.Coupon)
				.Include(b => b.Payments)
				.ToListAsync();
		}

		public async Task CreateBookingAsync(Booking booking)
		{
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();
		}

		public async Task<Booking?> GetBookingByIdAsync(int bookingId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.Guest)
				.Include(b => b.Room)
				.Include(b => b.Coupon)
				.Include(b => b.Payments)
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);
		}

		public async Task<Booking?> GetBookingByIdSimpleAsync(int bookingId)
		{
			return await _context.Bookings
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);
		}

		public async Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Where(b => b.GuestId == userId)
				.Include(b => b.Guest)
				.Include(b => b.Room)
				.Include(b => b.Coupon)
				.Include(b => b.Payments)
				.OrderByDescending(b => b.CheckIn)
				.ToListAsync();
		}

		public async Task<string?> GetLastBookingNumberByDateAsync(string datePrefix)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Where(b => b.OrderNumber != null && b.OrderNumber.StartsWith($"ORD{datePrefix}"))
				.OrderByDescending(b => b.OrderNumber)
				.Select(b => b.OrderNumber)
				.FirstOrDefaultAsync();
		}

		public async Task CancelBookingByIdAsync(Booking booking)
		{
			_context.Bookings.Update(booking);
			await _context.SaveChangesAsync();
		}
	}
}
