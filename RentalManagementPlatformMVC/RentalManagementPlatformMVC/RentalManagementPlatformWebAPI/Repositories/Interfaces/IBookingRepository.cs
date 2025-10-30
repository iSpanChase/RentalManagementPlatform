using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IBookingRepository
	{
		Task<IEnumerable<Booking>> GetAllBookingsAsync();
		Task<IEnumerable<Booking>> GetBookingsByGuestIdAsync(int guestId);
		Task<IEnumerable<Booking>> GetOrdersByHostIdAsync(int hostId);
		Task<Booking?> GetBookingByIdAsync(int bookingId);
		Task CreateBookingAsync(Booking booking);
		Task <Booking?> GetBookingByOrderNumberAsync(string orderNumber);
		Task UpdateBookingAsync(Booking booking);
		Task<string?> GetLastBookingNumberByDateAsync(string datePrefix);
	}
}
