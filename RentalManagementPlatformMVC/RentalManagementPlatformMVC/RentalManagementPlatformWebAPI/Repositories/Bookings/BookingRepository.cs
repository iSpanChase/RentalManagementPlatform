using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories.Bookings
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
				.Include(b => b.Coupon)
				.Include(b => b.Payments)
				.Include(b => b.Room)
					.ThenInclude(r => r.Host)
				.ToListAsync();
		}

		// 根據使用者ID獲取其所有訂單
		public async Task<IEnumerable<Booking>> GetBookingsByGuestIdAsync(int guestId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Where(b => b.GuestId == guestId)
				.Include(b => b.Room) // 同時載入房間資訊
				.Include(b => b.Guest) // 同時載入房客姓名
				.OrderByDescending(b => b.CreatedAt) // 讓最新的訂單在最前面
				.ToListAsync();
		}

		// 根據 HostId 獲取其所有訂單
		public async Task<IEnumerable<Booking>> GetOrdersByHostIdAsync(int hostId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.Room)
				.ThenInclude(r => r.Host)
				.Where(b => b.Room != null && b.Room.HostId == hostId)
				.Include(b => b.Guest)
				.OrderByDescending(b => b.CreatedAt)
				.ToListAsync();
		}

		// 根據 BookingId 取得訂單詳細資訊
		public async Task<Booking?> GetBookingByIdAsync(int bookingId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.Guest)
				.Include(b => b.Coupon)
				.Include(b => b.Payments)
				.Include(b => b.Room)
					.ThenInclude(r => r.Host)
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);
		}

		// 建立訂單
		public async Task CreateBookingAsync(Booking booking)
		{
			_context.Bookings.Add(booking);
			await _context.SaveChangesAsync();
		}

		// 根據 OrderNumber 取得訂單詳細資訊
		public async Task<Booking?> GetBookingByOrderNumberAsync(string orderNumber)
		{
			return await _context.Bookings
				.AsNoTracking()
				.FirstOrDefaultAsync(b => b.OrderNumber == orderNumber);
		}

		// 更新訂單
		public async Task UpdateBookingAsync(Booking booking)
		{
			_context.Bookings.Update(booking);
			await _context.SaveChangesAsync();
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
	}
}
