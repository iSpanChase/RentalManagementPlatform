using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public class HostSubscriptionRepository : IHostSubscriptionRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public HostSubscriptionRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 以分頁方式取得主機訂閱資料清單
		/// </summary>
		/// <param name="pageIndex">頁面索引（從1開始）</param>
		/// <param name="pageSize">每頁顯示的項目數量</param>
		/// <returns>包含主機訂閱資料集合和總筆數的元組</returns>
		public async Task<PagedResult<HostSubscription>> GetPagedHostSubscriptionsAsync(int pageIndex, int pageSize)
		{
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			var query = _context.HostSubscriptions
				.AsNoTracking()
				.Include(hs => hs.Host)
				.Include(hs => hs.Plan)
				.OrderByDescending(hs => hs.CreatedAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<HostSubscription>
			{
				Items = entities,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 根據主機訂閱ID取得詳細資訊
		/// </summary>
		/// <param name="hostSubscriptionId">主機訂閱的唯一識別碼</param>
		/// <returns>主機訂閱詳細資料，如果找不到則回傳null</returns>
		public async Task<HostSubscription?> GetHostSubscriptionDetailByIdAsync(int hostSubscriptionId)
		{
			return await _context.HostSubscriptions
				.AsNoTracking()
				.Include(hs => hs.Host)
				.Include(hs => hs.Plan)
				.Include(hs => hs.SubscriptionBillingLogs)
				.FirstOrDefaultAsync(hs => hs.HostSubId == hostSubscriptionId);
		}

		/// <summary>
		/// 動態條件查詢：根據篩選條件查詢主機訂閱
		/// 支援多種搜尋條件包含房東姓名、方案名稱、狀態、各種日期區間等
		/// 提供彈性的排序功能，並支援分頁查詢以提升效能
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含各種篩選參數</param>
		/// <param name="pageIndex">頁面索引，從1開始</param>
		/// <param name="pageSize">每頁筆數</param>
		/// <returns>回傳包含符合條件的訂閱清單與總筆數的元組</returns>
		public async Task<(IEnumerable<HostSubscription>, int)> SearchHostSubscriptionsAsync(HostSubscriptionSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 基底查詢
			var query = _context.HostSubscriptions
				.AsNoTracking()
				.Include(hs => hs.Host)
				.Include(hs => hs.Plan)
				.AsQueryable();

			// 房東ID篩選
			if (criteria.HostId.HasValue)
			{
				query = query.Where(hs => hs.HostId == criteria.HostId.Value);
			}

			// 方案ID篩選
			if (criteria.PlanId.HasValue)
			{
				query = query.Where(hs => hs.PlanId == criteria.PlanId.Value);
			}

			// 狀態篩選
			if (!string.IsNullOrWhiteSpace(criteria.Status))
			{
				var status = criteria.Status.Trim();
				query = query.Where(hs => hs.Status == status);
			}

			// 房東姓名搜尋（大小寫不敏感）
			if (!string.IsNullOrWhiteSpace(criteria.HostName))
			{
				var hostName = criteria.HostName.Trim();
				query = query.Where(hs => EF.Functions.Like(hs.Host.Name!, $"%{hostName}%"));
			}

			// 方案名稱搜尋（大小寫不敏感）
			if (!string.IsNullOrWhiteSpace(criteria.PlanName))
			{
				var planName = criteria.PlanName.Trim();
				query = query.Where(hs => EF.Functions.Like(hs.Plan.PlanName!, $"%{planName}%"));
			}

			// 開始日期範圍篩選
			if (criteria.StartDateFrom.HasValue)
			{
				query = query.Where(hs => hs.StartDate >= criteria.StartDateFrom.Value);
			}
			if (criteria.StartDateTo.HasValue)
			{
				var endOfDay = criteria.StartDateTo.Value.Date.AddDays(1);
				query = query.Where(hs => hs.StartDate < endOfDay);
			}

			// 建立時間範圍篩選
			if (criteria.CreatedAtFrom.HasValue)
			{
				query = query.Where(hs => hs.CreatedAt >= criteria.CreatedAtFrom.Value);
			}
			if (criteria.CreatedAtTo.HasValue)
			{
				var endOfDay = criteria.CreatedAtTo.Value.Date.AddDays(1);
				query = query.Where(hs => hs.CreatedAt < endOfDay);
			}

			// 下次計費日期範圍篩選
			if (criteria.NextBillingDateFrom.HasValue)
			{
				query = query.Where(hs => hs.NextBillingDate >= criteria.NextBillingDateFrom.Value);
			}
			if (criteria.NextBillingDateTo.HasValue)
			{
				var endOfDay = criteria.NextBillingDateTo.Value.Date.AddDays(1);
				query = query.Where(hs => hs.NextBillingDate < endOfDay);
			}

			// 排序
			query = criteria.SortBy?.ToLower() switch
			{
				"hostsubid" => criteria.IsDescending ? query.OrderByDescending(hs => hs.HostSubId) : query.OrderBy(hs => hs.HostSubId),
				"hostname" => criteria.IsDescending ? query.OrderByDescending(hs => hs.Host.Name) : query.OrderBy(hs => hs.Host.Name),
				"planname" => criteria.IsDescending ? query.OrderByDescending(hs => hs.Plan.PlanName) : query.OrderBy(hs => hs.Plan.PlanName),
				"startdate" => criteria.IsDescending ? query.OrderByDescending(hs => hs.StartDate) : query.OrderBy(hs => hs.StartDate),
				"nextbillingdate" => criteria.IsDescending ? query.OrderByDescending(hs => hs.NextBillingDate) : query.OrderBy(hs => hs.NextBillingDate),
				"status" => criteria.IsDescending ? query.OrderByDescending(hs => hs.Status) : query.OrderBy(hs => hs.Status),
				_ => criteria.IsDescending ? query.OrderByDescending(hs => hs.CreatedAt) : query.OrderBy(hs => hs.CreatedAt)
			};

			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return (entities, totalCount);
		}
	}
}
