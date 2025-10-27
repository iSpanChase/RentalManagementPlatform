using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IPaymentsRepository
	{
		// 取得所有付款紀錄(測試用)
		Task<IEnumerable<Payment>> GetAllPaymentsAsync();
		Task CreatePaymentAsync(Payment payment);
		Task<Payment?> GetPaymentByIdAsync(int paymentId);
		Task<IEnumerable<Payment>> GetPaymentsByHostIdAsync(int hostId);
	}
}
