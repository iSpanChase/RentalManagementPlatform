using RentalManagementPlatformWebAPI.DTOs.Bookings;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IBookingService
	{
		Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
		Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int guestId);
		Task<CreateOrderAndPayResponseDto> CreateBookingWithPaymentAsync(CreateBookingWithPaymentDto dto);
	}
}
