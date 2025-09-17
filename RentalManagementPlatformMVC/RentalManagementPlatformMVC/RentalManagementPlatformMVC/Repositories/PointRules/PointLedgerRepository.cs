using AutoMapper;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.PointRules
{
    public class PointLedgerRepository : IPointLedgerRepository
    {
        private readonly RentalManagementPlatformSqlContext _context;

		public PointLedgerRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 以分頁方式取得點數帳本資料清單
		/// </summary>
		/// <param name="pageIndex">頁面索引（從1開始）</param>
		/// <param name="pageSize">每頁顯示的項目數量</param>
		/// <returns>包含點數帳本資料集合和總筆數的元組</returns>
		public async Task<PagedResult<PointLedger>> GetPagedPointLedgersAsync(int pageIndex, int pageSize)
        {
			pageIndex = pageIndex <= 0 ? 1 : pageIndex;
			pageSize = pageSize <= 0 ? 20 : pageSize;

			var query = _context.PointLedgers
		        .AsNoTracking()
		        .Include(pl => pl.Guest)
		        .OrderByDescending(pl => pl.OccurredAt);

			var totalCount = await query.CountAsync();

			var entities = await query
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<PointLedger>
			{
				Items = entities,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

        /// <summary>
        /// 動態條件查詢：根據篩選條件查詢點數帳本
        /// 支援多種搜尋條件包含客戶ID、客戶姓名、點數類型、點數範圍、日期區間等
        /// 提供彈性的排序功能，並支援分頁查詢以提升效能
        /// </summary>
        /// <param name="criteria">搜尋條件物件，包含各種篩選參數</param>
        /// <param name="pageIndex">頁面索引，從1開始</param>
        /// <param name="pageSize">每頁筆數</param>
        /// <returns>回傳包含符合條件的點數帳本清單與總筆數的元組</returns>
        public async Task<(IEnumerable<PointLedger>, int)> SearchPointLedgersAsync(PointLedgerSearchCriteriaDto criteria, int pageIndex, int pageSize)
        {
            // 基底查詢
            var query = _context.PointLedgers
                .AsNoTracking()
                .Include(pl => pl.Guest)
                .AsQueryable();

            // 客戶ID篩選
            if (criteria.GuestId.HasValue)
            {
                query = query.Where(pl => pl.GuestId == criteria.GuestId.Value);
            }

            // 訂單號碼搜尋（大小寫不敏感）
            if (!string.IsNullOrWhiteSpace(criteria.OrderNumberSnapshot))
            {
                var orderNumber = criteria.OrderNumberSnapshot.Trim();
                query = query.Where(pl => EF.Functions.Like(pl.OrderNumberSnapshot!, $"%{orderNumber}%"));
            }

            // 客戶姓名搜尋（大小寫不敏感）
            if (!string.IsNullOrWhiteSpace(criteria.GuestName))
            {
                var guestName = criteria.GuestName.Trim();
                query = query.Where(pl => pl.Guest != null && EF.Functions.Like(pl.Guest.Name!, $"%{guestName}%"));
            }

            // 點數類型篩選
            if (!string.IsNullOrWhiteSpace(criteria.Type))
            {
                var type = criteria.Type.Trim();
                query = query.Where(pl => pl.Type == type);
            }

            // 點數範圍篩選
            if (criteria.PointsFrom.HasValue)
            {
                query = query.Where(pl => pl.Points >= criteria.PointsFrom.Value);
            }
            if (criteria.PointsTo.HasValue)
            {
                query = query.Where(pl => pl.Points <= criteria.PointsTo.Value);
            }

            // 發生時間範圍篩選
            if (criteria.OccurredAtFrom.HasValue)
            {
                query = query.Where(pl => pl.OccurredAt >= criteria.OccurredAtFrom.Value);
            }
            if (criteria.OccurredAtTo.HasValue)
            {
                var endOfDay = criteria.OccurredAtTo.Value.Date.AddDays(1);
                query = query.Where(pl => pl.OccurredAt < endOfDay);
            }

            // 到期時間範圍篩選
            if (criteria.ExpiresAtFrom.HasValue)
            {
                query = query.Where(pl => pl.ExpiresAt >= criteria.ExpiresAtFrom.Value);
            }
            if (criteria.ExpiresAtTo.HasValue)
            {
                var endOfDay = criteria.ExpiresAtTo.Value.Date.AddDays(1);
                query = query.Where(pl => pl.ExpiresAt < endOfDay);
            }

            // 是否過期篩選
            if (criteria.IsExpired.HasValue)
            {
                var now = DateTime.UtcNow;
                if (criteria.IsExpired.Value)
                {
                    query = query.Where(pl => pl.ExpiresAt.HasValue && pl.ExpiresAt.Value <= now);
                }
                else
                {
                    query = query.Where(pl => !pl.ExpiresAt.HasValue || pl.ExpiresAt.Value > now);
                }
            }

            // 排序
            query = criteria.SortBy?.ToLower() switch
            {
                "ledgerid" => criteria.IsDescending ? query.OrderByDescending(pl => pl.LedgerId) : query.OrderBy(pl => pl.LedgerId),
                "guestname" => criteria.IsDescending ? query.OrderByDescending(pl => pl.Guest != null ? pl.Guest.Name : "") : query.OrderBy(pl => pl.Guest != null ? pl.Guest.Name : ""),
                "type" => criteria.IsDescending ? query.OrderByDescending(pl => pl.Type) : query.OrderBy(pl => pl.Type),
                "points" => criteria.IsDescending ? query.OrderByDescending(pl => pl.Points) : query.OrderBy(pl => pl.Points),
                "expiresat" => criteria.IsDescending ? query.OrderByDescending(pl => pl.ExpiresAt) : query.OrderBy(pl => pl.ExpiresAt),
                _ => criteria.IsDescending ? query.OrderByDescending(pl => pl.OccurredAt) : query.OrderBy(pl => pl.OccurredAt)
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