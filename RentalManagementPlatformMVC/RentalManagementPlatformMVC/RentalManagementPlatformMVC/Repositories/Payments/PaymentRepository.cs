using Microsoft.EntityFrameworkCore;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.Payments;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.Payments
{
	public class PaymentRepository : IPaymentRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public PaymentRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 取得分頁的付款資料，依建立時間降序排列。
		/// </summary>
		/// <param name="pageIndex">頁面索引，從1開始</param>
		/// <param name="pageSize">每頁顯示的資料筆數</param>
		/// <returns>回傳包含付款資料集合與總筆數的元組</returns>
		public async Task<PagedResult<Payment>> GetPagedPaymentsAsync(int pageIndex, int pageSize)
		{
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			var query = _context.Payments
				.AsNoTracking()
				.OrderByDescending(p => p.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<Payment>
			{
				Items = entities,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 根據付款ID取得付款詳細資訊，包含交易記錄、訂房資訊、客人資料和房間資料。
		/// </summary>
		/// <param name="paymentId">付款的唯一識別碼</param>
		/// <returns>回傳包含完整關聯資料的付款物件，若找不到則回傳null</returns>
		public async Task<Payment?> GetPaymentDetailByIdAsync(int paymentId)
		{
			return await _context.Payments
				.AsNoTracking()
				.Include(p => p.PaymentTransactions)  // 詳細頁才抓交易集合
				.Include(p => p.Booking).ThenInclude(b => b.Guest)
				.Include(p => p.Booking).ThenInclude(b => b.Room)
				.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
		}

		/// <summary>
		/// 根據搜尋條件動態查詢付款資料，支援多種篩選條件、排序和分頁。
		/// 可依訂單編號、付款參考號、交易參考號、狀態、客人姓名、房間名稱、付款日期區間、金額區間等條件進行篩選，
		/// 並支援依付款日期、金額、狀態、付款參考號、訂單編號或建立日期進行升序或降序排列。
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含各種篩選參數如訂單編號、付款參考號、交易參考號、狀態、客人姓名、房間名稱、日期區間、金額區間、排序欄位及排序方向</param>
		/// <param name="pageIndex">頁面索引，從1開始計算，若傳入小於等於0的值將自動修正為1</param>
		/// <param name="pageSize">每頁顯示的資料筆數，若傳入小於等於0的值將自動修正為20</param>
		/// <returns>回傳包含符合條件的付款資料集合與總筆數的元組。付款資料包含關聯的訂房資訊、客人資料和房間資料</returns>
		public async Task<(IEnumerable<Payment>, int)> SearchPaymentsAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			var query = _context.Payments
				.AsNoTracking()
				.Include(p => p.Booking).ThenInclude(b => b.Guest)
				.Include(p => p.Booking).ThenInclude(b => b.Room)
				.AsQueryable();

			if (!string.IsNullOrWhiteSpace(criteria.OrderNumber))
			{
				var s = criteria.OrderNumber.Trim();
				query = query.Where(p => EF.Functions.Like(p.OrderNumberSnapshot!, $"%{s}%"));
			}

			if (!string.IsNullOrWhiteSpace(criteria.PaymentRef))
			{
				var s = criteria.PaymentRef.Trim();
				query = query.Where(p => p.PaymentRef == s);
			}

			if (!string.IsNullOrWhiteSpace(criteria.TransactionRef))
			{
				var s = criteria.TransactionRef.Trim();
				query = query.Where(p => p.PaymentTransactions.Any(t => t.TxnRef == s));
			}

			if (!string.IsNullOrWhiteSpace(criteria.Status))
			{
				var s = criteria.Status.Trim();
				query = query.Where(p => p.Status == s);
			}

			if (!string.IsNullOrWhiteSpace(criteria.GuestName))
			{
				var s = criteria.GuestName.Trim();
				query = query.Where(p => p.Booking != null &&
										 p.Booking.Guest != null &&
										 EF.Functions.Like(p.Booking.Guest.Name!, $"%{s}%"));
			}

			if (!string.IsNullOrWhiteSpace(criteria.RoomTitle))
			{
				var s = criteria.RoomTitle.Trim();
				query = query.Where(p => p.Booking != null &&
										 p.Booking.Room != null &&
										 EF.Functions.Like(p.Booking.Room.Title!, $"%{s}%"));
			}

			if (criteria.PaidStartDate.HasValue)
				query = query.Where(p => p.PaidAt >= criteria.PaidStartDate.Value);

			if (criteria.PaidEndDate.HasValue)
				query = query.Where(p => p.PaidAt <= criteria.PaidEndDate.Value);

			if (criteria.MinAmount.HasValue)
				query = query.Where(p => p.Amount >= criteria.MinAmount.Value);

			if (criteria.MaxAmount.HasValue)
				query = query.Where(p => p.Amount <= criteria.MaxAmount.Value);

			query = (criteria.SortBy ?? "createdat").ToLower() switch
			{
				"paidat" => criteria.IsDescending
					? query.OrderByDescending(p => p.PaidAt).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.PaidAt).ThenBy(p => p.PaymentId),

				"amount" => criteria.IsDescending
					? query.OrderByDescending(p => p.Amount).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.Amount).ThenBy(p => p.PaymentId),

				"status" => criteria.IsDescending
					? query.OrderByDescending(p => p.Status).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.Status).ThenBy(p => p.PaymentId),

				"paymentref" => criteria.IsDescending
					? query.OrderByDescending(p => p.PaymentRef).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.PaymentRef).ThenBy(p => p.PaymentId),

				"ordernumber" => criteria.IsDescending
					? query.OrderByDescending(p => p.OrderNumberSnapshot).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.OrderNumberSnapshot).ThenBy(p => p.PaymentId),

				_ => criteria.IsDescending
					? query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.PaymentId)
					: query.OrderBy(p => p.CreatedAt).ThenBy(p => p.PaymentId),
			};

			var totalCount = await query.CountAsync();

			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}
	}
}
