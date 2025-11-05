using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IBookingRepository
	{
		Task<IEnumerable<Booking>> GetMyBookingsAsync(int authenticatedGuestId);
		Task<IEnumerable<Booking>> GetMyOrdersAsync(int authenticatedHostId);
		Task<Booking?> GetBookingByIdAsync(int bookingId);
		Task CreateBookingAsync(Booking booking);
		Task <Booking?> GetBookingByOrderNumberAsync(string orderNumber);
		Task UpdateBookingAsync(Booking booking);
		Task<string?> GetLastBookingNumberByDateAsync(string datePrefix);
	}
}
