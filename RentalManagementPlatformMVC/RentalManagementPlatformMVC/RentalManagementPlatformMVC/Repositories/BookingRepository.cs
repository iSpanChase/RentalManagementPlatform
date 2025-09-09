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
		/// 提供基本的訂單分頁查詢功能，包含客戶資訊，依建立時間降序排列
		/// </summary>
		/// <param name="pageIndex">頁面索引，從1開始。若小於等於0則自動設為1</param>
		/// <param name="pageSize">每頁筆數，若小於等於0則自動設為20筆，最大限制100筆以確保效能</param>
		/// <returns>回傳包含訂單清單與總筆數的元組</returns>
		public async Task<(IEnumerable<Booking>, int)> GetPagedBookingsAsync(int pageIndex, int pageSize)
		{
			// 邊界值驗證
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : Math.Min(pageSize, 100); // 限制最大頁面大小為100

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
		/// 此方法會載入訂單的完整資訊，包含關聯的房客資料、優惠券、房間及房東資訊
		/// 使用 AsNoTracking() 提升查詢效能，適用於唯讀操作
		/// </summary>
		/// <param name="bookingId">訂單的唯一識別碼，用於查詢特定訂單</param>
		/// <returns>
		/// 回傳包含完整關聯資料的訂單物件，若找不到對應的訂單則回傳 null
		/// 包含的關聯資料：BookingGuests(訂房房客)、Guest(主要房客)、Coupon(優惠券)、Room(房間)、Host(房東)
		/// </returns>
		public async Task<Booking?> GetBookingDetailByIdAsync(int bookingId)
		{
			return await _context.Bookings
				.AsNoTracking()
				.Include(b => b.BookingGuests)
				.Include(b => b.Guest)
				.Include(b => b.Coupon)
				.Include(b => b.Room)
					.ThenInclude(r => r.Host)
				.FirstOrDefaultAsync(b => b.BookingId == bookingId);
		}

		/// <summary>
		/// 動態條件查詢：根據篩選條件查詢訂單
		/// 支援多種搜尋條件包含訂單編號、房客姓名、房間名稱、入住/退房日期區間、價格區間等
		/// 提供彈性的排序功能，並支援分頁查詢以提升效能
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含各種篩選參數如訂單編號、房客姓名、日期區間、價格區間等</param>
		/// <param name="pageIndex">頁面索引，從1開始。若小於等於0則自動設為1</param>
		/// <param name="pageSize">每頁筆數，若小於等於0則自動設為20筆</param>
		/// <returns>
		/// 回傳包含符合條件的訂單清單與總筆數的元組
		/// 訂單清單包含關聯的房客、房間、訂房房客資訊，依指定條件排序並分頁
		/// </returns>
		public async Task<(IEnumerable<Booking>, int)> SearchBookingsAsync(BookingSearchCriteriaDto criteria, int pageIndex, int pageSize)
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
