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

		/// <summary>
		/// 取得分頁的付款資料清單
		/// </summary>
		public async Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize)
		{
			var (entities, totalCount) = await _paymentRepository.GetPagedPaymentsAsync(pageIndex, pageSize);

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

				// 可選：顯示最新交易資訊
				// LatestTxnRef = p.PaymentTransactions
				//     .OrderByDescending(t => t.CreatedAt)
				//     .Select(t => t.TxnRef)
				//     .FirstOrDefault()
			}).ToList();

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 根據付款識別碼取得付款詳細資訊
		/// </summary>
		public async Task<PaymentDetailDto?> GetPaymentDetailByIdAsync(int paymentId)
		{
			var payment = await _paymentRepository.GetPaymentDetailByIdAsync(paymentId);
			if (payment == null) return null;

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

				// === 所有 Transactions ===
				Transactions = payment.PaymentTransactions
					.OrderByDescending(t => t.CreatedAt) // 依時間排序
					.Select(t => new PaymentTransactionDto
					{
						TransactionId = t.TransactionId,
						Provider = t.Provider,
						ProviderTxnId = t.ProviderTxnId,
						ResponseCode = t.ResponseCode,
						ResponseMessage = t.ResponseMessage,
						TxnRef = t.TxnRef,
						TransactionCreatedAt = t.CreatedAt
					})
					.ToList()
			};
		}

		/// <summary>
		/// 根據篩選條件動態查詢付款資料清單（支援分頁）
		/// </summary>
		public async Task<PagedResult<PaymentDto>> SearchPaymentsAsync(
			PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// tuple swap防止輸入值顛倒
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
			var (entities, total) = await _paymentRepository.SearchPaymentsAsync(criteria, pageIndex, pageSize);

			// 映射成列表用 DTO
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

				// 可選：顯示最新交易資訊
				// LatestTxnRef = p.PaymentTransactions
				//     .OrderByDescending(t => t.CreatedAt)
				//     .Select(t => t.TxnRef)
				//     .FirstOrDefault()
			}).ToList();

			return new PagedResult<PaymentDto>
			{
				Items = paymentDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = total
			};
		}
	}
}
