using RentalManagementPlatformWebAPI.DTOs.Bookings;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IBookingService
	{
		// 安全的方法 - 根據已驗證 GuestId 獲取其所有訂單
		Task<IEnumerable<BookingDto>> GetMyBookingsAsync(int authenticatedGuestId);
		// 安全的方法 - 根據已驗證 HostId 獲取其所有訂單
		Task<IEnumerable<BookingDto>> GetMyOrdersAsync(int authenticatedHostId);
		Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto);
		Task<BookingDto?> CancelBookingByIdAsync(int bookingId);
		Task<BookingDto?> GetBookingByOrderNumberAsync(string orderNumber); // New method
	}
}
