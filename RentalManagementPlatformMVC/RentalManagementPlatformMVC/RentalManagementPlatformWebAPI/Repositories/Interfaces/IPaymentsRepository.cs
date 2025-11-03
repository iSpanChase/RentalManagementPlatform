using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories.Interfaces
{
	public interface IPaymentsRepository
	{
		Task CreatePaymentAsync(Payment payment);
	}
}
