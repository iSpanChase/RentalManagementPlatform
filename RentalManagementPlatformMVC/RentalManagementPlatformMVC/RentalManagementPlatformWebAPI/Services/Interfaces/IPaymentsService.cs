using RentalManagementPlatformWebAPI.DTOs.Payments;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface IPaymentsService
	{
		Task<PaymentCallbackResultDto> ProcessEcpayCallbackAsync(
			string orderNumber,
			bool isSuccess,
			string tradeNo,
			decimal tradeAmount,
			DateTime paymentDate,
			string paymentType);
		Task<string> GeneratePaymentFormForDeferredBookingAsync(string orderNumber);
	}
}
