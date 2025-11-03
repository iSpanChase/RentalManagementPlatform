using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IBookingRepository
	{
		// 不安全的方法 - 根據 GuestId 獲取其所有訂單
		Task<IEnumerable<Booking>> GetBookingsByGuestIdAsync(int guestId);
		// 不安全的方法 - 根據 HostId 獲取其所有訂單
		Task<IEnumerable<Booking>> GetOrdersByHostIdAsync(int hostId);
		// 安全方法 - 從 claims 取得房客身份
		Task<IEnumerable<Booking>> GetMyBookingsAsync(int authenticatedGuestId);
		Task<IEnumerable<Booking>> GetMyOrdersAsync(int authenticatedHostId);

		Task<Booking?> GetBookingByIdAsync(int bookingId);
		Task CreateBookingAsync(Booking booking);
		Task <Booking?> GetBookingByOrderNumberAsync(string orderNumber);
		Task UpdateBookingAsync(Booking booking);
		Task<string?> GetLastBookingNumberByDateAsync(string datePrefix);
	}
}
