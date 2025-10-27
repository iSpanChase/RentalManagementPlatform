using RentalManagementPlatformWebAPI.DTOs.Bookings;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IBookingService
	{
		// 取得所有訂單(測試用)
		Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
		Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto);
		Task<BookingDto?> GetBookingByIdAsync(int bookingId);
		Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int userId);
		Task<BookingDto?> CancelBookingByIdAsync(int bookingId);
	}
}
