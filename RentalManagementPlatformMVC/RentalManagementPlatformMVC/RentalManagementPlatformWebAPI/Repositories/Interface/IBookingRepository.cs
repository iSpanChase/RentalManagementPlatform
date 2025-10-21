using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interface
{
	public interface IBookingRepository
	{
		// 取得所有訂單(測試用)
		Task<IEnumerable<Booking>> GetAllBookingsAsync();
		Task CreateBookingAsync(Booking booking);
		Task<Booking?> GetBookingByIdAsync(int bookingId);
		Task<IEnumerable<Booking>> GetBookingsByUserAsync(int userId);
		Task<string?> GetLastBookingNumberByDateAsync(string datePrefix);
		Task CancelBookingByIdAsync(Booking booking);
		Task<Booking?> GetBookingByIdSimpleAsync(int bookingId);
	}
}
