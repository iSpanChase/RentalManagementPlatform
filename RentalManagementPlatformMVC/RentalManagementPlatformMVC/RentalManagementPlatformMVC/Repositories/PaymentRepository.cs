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

		public async Task<(IEnumerable<Payment>, int)> GetPagedPaymentAsync(int pageIndex, int pageSize)
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

		public async Task<Payment?> GetByIdAsync(int paymentId)
		{
			return await _context.Payments
				.AsNoTracking()
				.Include(p => p.PaymentTransaction)
				.Include(p => p.Booking)
					.ThenInclude(b => b.Guest)
				.Include(p => p.Booking)
					.ThenInclude(b => b.Room)
				.FirstOrDefaultAsync(p => p.PaymentId == paymentId);
		}

		public async Task<(IEnumerable<Payment>, int)> SearchAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 基底查詢
			var query = _context.Payments
				.AsNoTracking()
				.Include(p => p.PaymentTransaction)
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

			// 交易參考號
			if (!string.IsNullOrWhiteSpace(criteria.TransactionRef))
			{
				var s = criteria.TransactionRef.Trim();
				query = query.Where(p => p.PaymentTransaction != null &&
										 p.PaymentTransaction.TxnRef == s);
			}

			// 狀態（paid/pending/refunded/failed）
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

			// 日期區間（付款時間：PaidAt；含端點）
			if (criteria.PaidStartDate.HasValue)
				query = query.Where(p => p.PaidAt >= criteria.PaidStartDate.Value);

			if (criteria.PaidEndDate.HasValue)
				query = query.Where(p => p.PaidAt <= criteria.PaidEndDate.Value);

			// 金額區間（Payment.Amount）
			if (criteria.MinAmount.HasValue)
				query = query.Where(p => p.Amount >= criteria.MinAmount.Value);

			if (criteria.MaxAmount.HasValue)
				query = query.Where(p => p.Amount <= criteria.MaxAmount.Value);

			// 排序（預設 CreatedAt）
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
