using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;

namespace RentalManagementPlatformWebAPI.Repositories.Payments
{
	public class PaymentsRepository : IPaymentsRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;
		private readonly ILogger<PaymentsRepository> _logger;

		public PaymentsRepository(RentalManagementPlatformSqlContext context, ILogger<PaymentsRepository> logger)
		{
			_context = context;
			_logger = logger;
		}

		public async Task CreatePaymentAsync(Payment payment)
		{
			try
			{
				_logger.LogInformation("=== PaymentsRepository: 開始建立 Payment 記錄 ===");
				_logger.LogInformation($"BookingId: {payment.BookingId}");
				_logger.LogInformation($"Amount: {payment.Amount}");
				_logger.LogInformation($"Method: {payment.Method}");
				_logger.LogInformation($"PaymentRef: {payment.PaymentRef}");
				_logger.LogInformation($"Status: {payment.Status}");

				_context.Payments.Add(payment);
				await _context.SaveChangesAsync();

				_logger.LogInformation($"Payment 記錄已成功建立：PaymentId={payment.PaymentId}");
			}
			catch (Exception ex)
			{
				_logger.LogError($"建立 Payment 記錄失敗：{ex.Message}");
				_logger.LogError($"錯誤詳情：{ex.InnerException?.Message}");
				_logger.LogError($"堆疊追蹤：{ex.StackTrace}");
				_logger.LogError($"PaymentData: BookingId={payment.BookingId}, Amount={payment.Amount}, Status={payment.Status}");
				throw;
			}
		}
	}
}
