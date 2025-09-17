using AutoMapper;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Repositories.PointRules;

namespace RentalManagementPlatformMVC.Services.PointRules
{
    public class PointLedgerService : IPointLedgerService
    {
        private readonly IPointLedgerRepository _pointLedgerRepository;
        private readonly IMapper _mapper;

        public PointLedgerService(IPointLedgerRepository pointLedgerRepository, IMapper mapper)
        {
            _pointLedgerRepository = pointLedgerRepository;
            _mapper = mapper;
        }

        /// <summary>
        /// 初始載入：取得所有點數帳本資料（支援分頁）
        /// </summary>
        /// <param name="pageIndex">頁面索引（從1開始）</param>
        /// <param name="pageSize">每頁顯示的項目數量</param>
        /// <returns>分頁的點數帳本資料集合</returns>
        public async Task<PagedResult<PointLedgerDto>> GetPagedPointLedgersAsync(int pageIndex, int pageSize)
        {
            var pagedEntities = await _pointLedgerRepository.GetPagedPointLedgersAsync(pageIndex, pageSize);

            var pointLedgerDtos = _mapper.Map<List<PointLedgerDto>>(pagedEntities.Items);

            return new PagedResult<PointLedgerDto>
            {
				Items = pointLedgerDtos,
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
        }

        /// <summary>
        /// 動態條件查詢：根據篩選條件查詢點數帳本資料（支援分頁）
        /// </summary>
        /// <param name="criteria">搜尋條件物件，包含各種篩選參數</param>
        /// <param name="pageIndex">頁面索引（從1開始）</param>
        /// <param name="pageSize">每頁顯示的項目數量</param>
        /// <returns>符合條件的分頁點數帳本資料集合</returns>
        public async Task<PagedResult<PointLedgerDto>> SearchPointLedgersAsync(PointLedgerSearchCriteriaDto criteria, int pageIndex, int pageSize)
        {
            // 日期範圍驗證與自動調整
            if (criteria.OccurredAtFrom.HasValue && criteria.OccurredAtTo.HasValue &&
                criteria.OccurredAtFrom > criteria.OccurredAtTo)
            {
                (criteria.OccurredAtFrom, criteria.OccurredAtTo) = (criteria.OccurredAtTo, criteria.OccurredAtFrom);
            }

            if (criteria.ExpiresAtFrom.HasValue && criteria.ExpiresAtTo.HasValue &&
                criteria.ExpiresAtFrom > criteria.ExpiresAtTo)
            {
                (criteria.ExpiresAtFrom, criteria.ExpiresAtTo) = (criteria.ExpiresAtTo, criteria.ExpiresAtFrom);
            }

            if (criteria.PointsFrom.HasValue && criteria.PointsTo.HasValue &&
                criteria.PointsFrom > criteria.PointsTo)
            {
                (criteria.PointsFrom, criteria.PointsTo) = (criteria.PointsTo, criteria.PointsFrom);
            }

            // 呼叫 Repository 動態查詢
            var (entities, totalCount) = await _pointLedgerRepository.SearchPointLedgersAsync(criteria, pageIndex, pageSize);

            // 映射成 DTO
            var pointLedgerDtos = _mapper.Map<List<PointLedgerDto>>(entities);

            return new PagedResult<PointLedgerDto>
            {
                Items = pointLedgerDtos,
                PageIndex = pageIndex,
                PageSize = pageSize,
                TotalCount = totalCount
            };
        }
    }
}