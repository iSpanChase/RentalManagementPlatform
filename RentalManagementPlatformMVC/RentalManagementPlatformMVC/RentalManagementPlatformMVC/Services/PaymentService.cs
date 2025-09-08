using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Repositories;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
	public class PaymentService : IPaymentService
	{
		private readonly IPaymentRepository _paymentRepository;
		
		public PaymentService(IPaymentRepository paymentRepository)
		{
			_paymentRepository = paymentRepository;
		}

		public async Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize)
		{
			var (entities, totalCount) = await _paymentRepository.GetPagedPaymentAsync(pageIndex, pageSize);

			var paymentDtos = entities.Select(p => new PaymentDto
			{
				PaymentId = p.PaymentId,
				OrderNumberSnapshot = p.OrderNumberSnapshot,
				Amount = p.Amount,
				PaymentRef = p.PaymentRef,
				Method = p.Method,
				PaidAt = p.PaidAt,
				Status = p.Status,
				CreatedAt = p.CreatedAt,
			}).ToList();

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		public async Task<PaymentDetailDto?> GetPaymentDetailByIdAsync(int paymentId)
		{
			var payment = await _paymentRepository.GetByIdAsync(paymentId);
			if (payment == null) return null;

			// 先安全取出 Transaction
			var transaction = payment.PaymentTransaction;

			return new PaymentDetailDto
			{
				// === Payment ===
				PaymentId = payment.PaymentId,
				BookingId = payment.BookingId,
				Amount = payment.Amount,
				Method = payment.Method,
				PaidAt = payment.PaidAt,
				PaymentRef = payment.PaymentRef,
				OrderNumberSnapshot = payment.OrderNumberSnapshot,
				Status = payment.Status,
				PaymentCreatedAt = payment.CreatedAt,
				GuestName = payment.Booking?.Guest?.Name, 
				RoomTitle = payment.Booking?.Room?.Title,

				// === Transaction ===
				TransactionId = transaction?.TransactionId ?? 0,
				Provider = transaction?.Provider,
				ProviderTxnId = transaction?.ProviderTxnId,
				ResponseCode = transaction?.ResponseCode,
				ResponseMessage = transaction?.ResponseMessage,
				TxnRef = transaction?.TxnRef,
				TransactionCreatedAt = transaction?.CreatedAt
			};
		}

		public async Task<PagedResult<PaymentDto>> SearchPaymentsAsync(
	PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 可選：規格修正（避免使用者傳錯）
			if (criteria.MinAmount.HasValue && criteria.MaxAmount.HasValue &&
				criteria.MinAmount > criteria.MaxAmount)
			{
				(criteria.MinAmount, criteria.MaxAmount) = (criteria.MaxAmount, criteria.MinAmount);
			}

			if (criteria.PaidStartDate.HasValue && criteria.PaidEndDate.HasValue &&
				criteria.PaidStartDate > criteria.PaidEndDate)
			{
				(criteria.PaidStartDate, criteria.PaidEndDate) = (criteria.PaidEndDate, criteria.PaidStartDate);
			}

			// 呼叫 Repository 動態查詢
			var (entities, total) = await _paymentRepository.SearchAsync(criteria, pageIndex, pageSize);

			// 映射成列表用 DTO（沿用現有 PaymentDto 欄位）
			var items = entities.Select(p => new PaymentDto
			{
				PaymentId = p.PaymentId,
				OrderNumberSnapshot = p.OrderNumberSnapshot,
				Amount = p.Amount,
				PaymentRef = p.PaymentRef,
				Method = p.Method,
				PaidAt = p.PaidAt,
				Status = p.Status,
				CreatedAt = p.CreatedAt,
				// 若未來需要也可擴充（你的 DTO 若有這些欄位再打開）
				// GuestName         = p.Booking?.Guest?.Name,
				// RoomTitle         = p.Booking?.Room?.Title,
				// TransactionRef    = p.PaymentTransaction?.TxnRef,
				// ProviderTxnId     = p.PaymentTransaction?.ProviderTxnId,
			}).ToList();

			return new PagedResult<PaymentDto>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = total
			};
		}
	}
}
