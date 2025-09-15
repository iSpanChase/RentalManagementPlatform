using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.DTOs.Payments;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.Payments
{
	public class HostPayoutRepository : IHostPayoutRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public HostPayoutRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 取得分頁的主機付款資料
		/// </summary>
		/// <param name="pageIndex">頁碼索引（從1開始）</param>
		/// <param name="pageSize">每頁筆數</param>
		/// <returns>回傳包含主機付款資料集合和總筆數的元組</returns>
		public async Task<(IEnumerable<HostPayout>, int)> GetPagedHostPayoutsAsync(int pageIndex, int pageSize)
		{
			var query = _context.HostPayouts
				.AsNoTracking()
				.Include(p => p.Host)
				.OrderByDescending(p => p.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}

		/// <summary>
		/// 根據主機付款ID取得詳細資訊，包含相關的付款項目
		/// </summary>
		/// <param name="hostPayoutId">主機付款ID</param>
		/// <returns>回傳主機付款詳細資訊，若找不到則回傳null</returns>
		public async Task<HostPayout?> GetHostPayoutDetailByIdAsync(int hostPayoutId)
		{
			return await _context.HostPayouts
				.AsNoTracking()
				.Include(p => p.Host)
				.FirstOrDefaultAsync(p => p.PayoutId == hostPayoutId);
		}

		/// <summary>
		/// 根據搜尋條件動態查詢主機付款資料，支援分頁功能
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含主機ID、主機名稱、付款日期範圍、金額範圍、排序方式等篩選條件</param>
		/// <param name="pageIndex">頁碼索引（從1開始）</param>
		/// <param name="pageSize">每頁筆數</param>
		/// <returns>回傳包含符合條件的主機付款資料集合和總筆數的元組</returns>
		public async Task<(IEnumerable<HostPayout>, int)> SearchHostPayoutsAsync(HostSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			var query = _context.HostPayouts
				.AsNoTracking()
				.Include(p => p.Host)
				.AsQueryable();

			if (criteria.HostId.HasValue)
			{
				query = query.Where(p => p.HostId == criteria.HostId.Value);
			}

			if (!string.IsNullOrWhiteSpace(criteria.HostName))
			{
				var s = criteria.HostName.Trim();
				query = query.Where(p => p.Host != null && EF.Functions.Like(p.Host.Name!, $"%{s}%"));
			}

			if (criteria.PaidStartDate.HasValue)
				query = query.Where(p => p.PaidAt >= criteria.PaidStartDate.Value);

			if (criteria.PaidEndDate.HasValue)
				query = query.Where(p => p.PaidAt <= criteria.PaidEndDate.Value);

			if (criteria.MinAmount.HasValue)
				query = query.Where(p => p.AmountGross >= criteria.MinAmount.Value);

			if (criteria.MaxAmount.HasValue)
				query = query.Where(p => p.AmountGross <= criteria.MaxAmount.Value);

			query = (criteria.SortBy ?? "createdat").ToLower() switch
			{
				"paidat" => criteria.IsDescending
					? query.OrderByDescending(p => p.PaidAt).ThenByDescending(p => p.PayoutId)
					: query.OrderBy(p => p.PaidAt).ThenBy(p => p.PayoutId),

				"amount" => criteria.IsDescending
					? query.OrderByDescending(p => p.AmountGross).ThenByDescending(p => p.PayoutId)
					: query.OrderBy(p => p.AmountGross).ThenBy(p => p.PayoutId),

				"status" => criteria.IsDescending
					? query.OrderByDescending(p => p.Status).ThenByDescending(p => p.PayoutId)
					: query.OrderBy(p => p.Status).ThenBy(p => p.PayoutId),

				_ => criteria.IsDescending
					? query.OrderByDescending(p => p.CreatedAt).ThenByDescending(p => p.PayoutId)
					: query.OrderBy(p => p.CreatedAt).ThenBy(p => p.PayoutId),
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
