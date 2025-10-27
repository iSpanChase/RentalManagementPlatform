using RentalManagementPlatformWebAPI.DTOs.Bookings;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IPaymentsService
	{
		// 取得所有付款紀錄(測試用)
		Task<IEnumerable<PaymentsDto>> GetAllPaymentsAsync();
		Task<PaymentsDto> CreatePaymentAsync(CreatePaymentDto dto);
		Task<PaymentsDto?> GetPaymentByIdAsync(int paymentId);
		Task<IEnumerable<PaymentsDto>> GetPaymentsByHostIdAsync(int hostId);
	}
}
