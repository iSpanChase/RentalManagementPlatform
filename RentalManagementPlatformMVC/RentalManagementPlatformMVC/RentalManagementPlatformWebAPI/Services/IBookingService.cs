using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOs;

namespace RentalManagementPlatformWebAPI.Services
{
	public interface IBookingService
	{
		// 取得所有訂單(測試用)
		Task<IEnumerable<BookingDto>> GetAllBookingsAsync();
		Task<BookingDto> CreateBookingAsync(CreateBookingDto dto);
		Task<BookingDto?> GetBookingByIdAsync(int bookingId);
		Task<IEnumerable<BookingDto>> GetBookingsByUserAsync(int userId);
	}
}
