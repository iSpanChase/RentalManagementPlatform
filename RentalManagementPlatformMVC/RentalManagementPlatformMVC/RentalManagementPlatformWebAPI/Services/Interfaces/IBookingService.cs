using RentalManagementPlatformWebAPI.DTOs.Bookings;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IBookingService
	{
		Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int guestId);
		Task<IEnumerable<BookingDto>> GetOrdersByHostIdAsync(int hostId);
		Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto);
		Task<BookingDto?> CancelBookingByIdAsync(int bookingId);
		Task<BookingDto?> GetBookingByOrderNumberAsync(string orderNumber); // New method
	}
}
