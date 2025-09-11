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
		/// <param name="pageIndex">頁面索引，從 0 開始計算</param>
		/// <param name="pageSize">每頁顯示的資料筆數</param>
		/// <returns>回傳包含付款資料清單及分頁資訊的 PagedResult 物件</returns>
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
		/// <param name="paymentId">付款識別碼，用於查詢特定付款記錄的唯一標識符</param>
		/// <returns>回傳付款詳細資訊的 DTO 物件，包含付款基本資料、關聯的訂房資訊、客人姓名、房間標題及所有相關交易紀錄。若查無此付款記錄則回傳 null</returns>
		public async Task<PaymentDetailDto?> GetPaymentDetailByIdAsync(int paymentId)
		{
			var payment = await _paymentRepository.GetPaymentDetailByIdAsync(paymentId);
			if (payment == null) return null;

			return new PaymentDetailDto
			{
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
					}).ToList() ?? new List<PaymentTransactionDto>()
			};
		}

		/// <summary>
		/// 根據篩選條件動態查詢付款資料清單（支援分頁）
		/// </summary>
		/// <param name="criteria">篩選條件物件，包含訂單編號、付款參考號、交易參考號、狀態、客人姓名、房間名稱、付款日期區間、金額區間及排序設定等查詢條件</param>
		/// <param name="pageIndex">頁面索引，從 0 開始計算，用於指定要取得的頁面位置</param>
		/// <param name="pageSize">每頁顯示的資料筆數，用於控制單頁回傳的資料量</param>
		/// <returns>回傳包含符合篩選條件的付款資料清單及分頁資訊的 PagedResult 物件，若查無符合條件的資料則回傳空的資料清單</returns>
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
