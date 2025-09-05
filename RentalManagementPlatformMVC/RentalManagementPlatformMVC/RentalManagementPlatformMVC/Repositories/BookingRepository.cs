using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public class BookingRepository : IBookingRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public BookingRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 初始載入：取得所有訂單資料（支援分頁）
		/// </summary>
		public async Task<(IEnumerable<Booking>, int)> GetPagedBookingsAsync(int pageIndex, int pageSize)
		{
			var query = _context.Bookings
				.AsNoTracking()
				.Include(b => b.Guest)
				.OrderByDescending(b => b.CreatedAt);

			var totalCount = await query.CountAsync();

			var items = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (items, totalCount);
		}

		/// <summary>
		/// 詳細頁面：根據訂單ID取得訂單詳細資訊
		/// </summary>
		public async Task<Booking?> GetByIdAsync(int bookingId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.BookingGuests)
				.Include(b => b.Guest)
				.Include(b => b.Coupon)
				.Include(b => b.Room)
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);
		}

		/// <summary>
		/// 動態條件查詢：根據篩選條件查詢訂單
		/// </summary>
		public async Task<(IEnumerable<Booking>, int)> SearchAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 基底查詢
			var query = _context.Bookings
				.AsNoTracking()
				.Include(b => b.Guest)
				.Include(b => b.Room)
				.Include(b => b.BookingGuests) // 需要用到 Count
				.AsQueryable();

			// 關鍵字（大小寫不敏感，使用 EF.Functions.Like 讓 SQL 端執行）
			if (!string.IsNullOrWhiteSpace(criteria.OrderNumber))
			{
				var s = criteria.OrderNumber.Trim();
				query = query.Where(b => EF.Functions.Like(b.OrderNumber!, $"%{s}%"));
			}

			if (!string.IsNullOrWhiteSpace(criteria.Status))
			{
				var s = criteria.Status.Trim();
				query = query.Where(b => b.Status == s);
			}

			if (!string.IsNullOrWhiteSpace(criteria.GuestName))
			{
				var s = criteria.GuestName.Trim();
				query = query.Where(b => EF.Functions.Like(b.Guest!.Name!, $"%{s}%"));
			}

			if (!string.IsNullOrWhiteSpace(criteria.Room))
			{
				var s = criteria.Room.Trim();
				query = query.Where(b => EF.Functions.Like(b.Room!.Title!, $"%{s}%"));
			}

			// 日期區間（含端點）
			if (criteria.CheckInStartDate.HasValue)
				query = query.Where(b => b.CheckIn >= criteria.CheckInStartDate.Value);

			if (criteria.CheckInEndDate.HasValue)
				query = query.Where(b => b.CheckIn <= criteria.CheckInEndDate.Value);

			if (criteria.CheckOutStartDate.HasValue)
				query = query.Where(b => b.CheckOut >= criteria.CheckOutStartDate.Value);

			if (criteria.CheckOutEndDate.HasValue)
				query = query.Where(b => b.CheckOut <= criteria.CheckOutEndDate.Value);

			// 價格區間
			if (criteria.MinPrice.HasValue)
				query = query.Where(b => b.TotalPrice >= criteria.MinPrice.Value);

			if (criteria.MaxPrice.HasValue)
				query = query.Where(b => b.TotalPrice <= criteria.MaxPrice.Value);

			// 排序
			query = (criteria.SortBy ?? "createdat").ToLower() switch
			{
				"checkin" => criteria.IsDescending
					? query.OrderByDescending(b => b.CheckIn).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.CheckIn).ThenBy(b => b.BookingId),

				"checkout" => criteria.IsDescending
					? query.OrderByDescending(b => b.CheckOut).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.CheckOut).ThenBy(b => b.BookingId),

				"totalprice" => criteria.IsDescending
					? query.OrderByDescending(b => b.TotalPrice).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.TotalPrice).ThenBy(b => b.BookingId),

				"status" => criteria.IsDescending
					? query.OrderByDescending(b => b.Status).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.Status).ThenBy(b => b.BookingId),

				"ordernumber" => criteria.IsDescending
					? query.OrderByDescending(b => b.OrderNumber).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.OrderNumber).ThenBy(b => b.BookingId),

				_ => criteria.IsDescending
					? query.OrderByDescending(b => b.CreatedAt).ThenByDescending(b => b.BookingId)
					: query.OrderBy(b => b.CreatedAt).ThenBy(b => b.BookingId),
			};

			// 先算總筆數
			var totalCount = await query.CountAsync();

			// 分頁邊界修正
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			// 取當頁資料（回傳實體）
			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}
	}
}
