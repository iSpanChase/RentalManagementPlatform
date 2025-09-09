using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
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
		public async Task<(IEnumerable<Payment>, int)> GetPagedPaymentsAsync(int pageIndex, int pageSize)
		{
			var query = _context.Payments
				.AsNoTracking()
				.OrderByDescending(p => p.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}

		/// <summary>
		/// 根據付款ID取得付款詳細資訊，包含交易記錄、訂房資訊、客人資料和房間資料。
		/// </summary>
		public async Task<Payment?> GetPaymentDetailByIdAsync(int paymentId)
		{
			return await _context.Payments
				.AsNoTracking()
				.Include(p => p.PaymentTransactions) // ✅ 詳細頁才抓交易集合
				.Include(p => p.Booking).ThenInclude(b => b.Guest)
				.Include(p => p.Booking).ThenInclude(b => b.Room)
				.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
		}

		/// <summary>
		/// 根據搜尋條件動態查詢付款資料，支援多種篩選條件、排序和分頁。
		/// </summary>
		public async Task<(IEnumerable<Payment>, int)> SearchPaymentsAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 基底查詢（❌ 不要 Include PaymentTransactions，避免乘出）
			var query = _context.Payments
				.AsNoTracking()
				.Include(p => p.Booking).ThenInclude(b => b.Guest)
				.Include(p => p.Booking).ThenInclude(b => b.Room)
				.AsQueryable();

			// 訂單編號
			if (!string.IsNullOrWhiteSpace(criteria.OrderNumber))
			{
				var s = criteria.OrderNumber.Trim();
				query = query.Where(p => EF.Functions.Like(p.OrderNumberSnapshot!, $"%{s}%"));
			}

			// 付款參考號
			if (!string.IsNullOrWhiteSpace(criteria.PaymentRef))
			{
				var s = criteria.PaymentRef.Trim();
				query = query.Where(p => p.PaymentRef == s);
			}

			// 交易參考號（✅ 改用 Any）
			if (!string.IsNullOrWhiteSpace(criteria.TransactionRef))
			{
				var s = criteria.TransactionRef.Trim();
				query = query.Where(p => p.PaymentTransactions.Any(t => t.TxnRef == s));
			}

			// 狀態
			if (!string.IsNullOrWhiteSpace(criteria.Status))
			{
				var s = criteria.Status.Trim();
				query = query.Where(p => p.Status == s);
			}

			// 訂房姓名
			if (!string.IsNullOrWhiteSpace(criteria.GuestName))
			{
				var s = criteria.GuestName.Trim();
				query = query.Where(p => p.Booking != null &&
										 p.Booking.Guest != null &&
										 EF.Functions.Like(p.Booking.Guest.Name!, $"%{s}%"));
			}

			// 房間名稱
			if (!string.IsNullOrWhiteSpace(criteria.RoomTitle))
			{
				var s = criteria.RoomTitle.Trim();
				query = query.Where(p => p.Booking != null &&
										 p.Booking.Room != null &&
										 EF.Functions.Like(p.Booking.Room.Title!, $"%{s}%"));
			}

			// 日期區間
			if (criteria.PaidStartDate.HasValue)
				query = query.Where(p => p.PaidAt >= criteria.PaidStartDate.Value);

			if (criteria.PaidEndDate.HasValue)
				query = query.Where(p => p.PaidAt <= criteria.PaidEndDate.Value);

			// 金額區間
			if (criteria.MinAmount.HasValue)
				query = query.Where(p => p.Amount >= criteria.MinAmount.Value);

			if (criteria.MaxAmount.HasValue)
				query = query.Where(p => p.Amount <= criteria.MaxAmount.Value);

			// 排序
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

			// 總筆數
			var totalCount = await query.CountAsync();

			// 分頁邊界修正
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			// 取當頁資料
			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}
	}
}
