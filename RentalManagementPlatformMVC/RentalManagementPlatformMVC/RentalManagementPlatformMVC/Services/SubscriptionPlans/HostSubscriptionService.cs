using AutoMapper;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using RentalManagementPlatformMVC.Repositories.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Services.SubscriptionPlans
{
	public class HostSubscriptionService : IHostSubscriptionService
	{
		private readonly IHostSubscriptionRepository _hostSubscriptionRepository;
		private readonly IMapper _mapper;
		public HostSubscriptionService(IHostSubscriptionRepository hostSubscriptionRepository, IMapper mapper)
		{
			_hostSubscriptionRepository = hostSubscriptionRepository;
			_mapper = mapper;
		}
		public async Task<PagedResult<HostSubscriptionDto>> GetPagedHostSubscriptionsAsync(int pageIndex, int pageSize)
		{
			var pagedEntities = await _hostSubscriptionRepository.GetPagedHostSubscriptionsAsync(pageIndex, pageSize);

			var hostPlanDto = _mapper.Map<List<HostSubscriptionDto>>(pagedEntities.Items);

			return new PagedResult<HostSubscriptionDto>
			{
				Items = hostPlanDto,
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
		}

		public async Task<HostSubscriptionDetailDto?> GetHostSubscriptionByIdAsync(int hostSubscriptionId)
		{
			var entity = await _hostSubscriptionRepository.GetHostSubscriptionDetailByIdAsync(hostSubscriptionId);

			if (entity == null) return null;

			var hostPlanDto = _mapper.Map<HostSubscriptionDetailDto>(entity);

			return hostPlanDto;
		}

		public async Task<PagedResult<HostSubscriptionDto>> SearchHostSubscriptionsAsync(HostSubscriptionSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// 日期範圍驗證與自動調整
			if (criteria.StartDateFrom.HasValue && criteria.StartDateTo.HasValue &&
				criteria.StartDateFrom > criteria.StartDateTo)
			{
				(criteria.StartDateFrom, criteria.StartDateTo) = (criteria.StartDateTo, criteria.StartDateFrom);
			}

			if (criteria.CreatedAtFrom.HasValue && criteria.CreatedAtTo.HasValue &&
				criteria.CreatedAtFrom > criteria.CreatedAtTo)
			{
				(criteria.CreatedAtFrom, criteria.CreatedAtTo) = (criteria.CreatedAtTo, criteria.CreatedAtFrom);
			}

			if (criteria.NextBillingDateFrom.HasValue && criteria.NextBillingDateTo.HasValue &&
				criteria.NextBillingDateFrom > criteria.NextBillingDateTo)
			{
				(criteria.NextBillingDateFrom, criteria.NextBillingDateTo) = (criteria.NextBillingDateTo, criteria.NextBillingDateFrom);
			}

			// 呼叫 Repository 動態查詢
			var (entities, totalCount) = await _hostSubscriptionRepository.SearchHostSubscriptionsAsync(criteria, pageIndex, pageSize);

			// 映射成列表用 DTO
			var hostSubscriptionDtos = entities.Select(hs => new HostSubscriptionDto
			{
				HostSubId = hs.HostSubId,
				HostId = hs.HostId,
				PlanId = hs.PlanId,
				StartDate = hs.StartDate,
				NextBillingDate = hs.NextBillingDate,
				CancelAtPeriodEnd = hs.CancelAtPeriodEnd,
				Status = hs.Status,
				CreatedAt = hs.CreatedAt,
				HostName = hs.Host?.Name,
				PlanName = hs.Plan?.PlanName
			}).ToList();

			return new PagedResult<HostSubscriptionDto>
			{
				Items = hostSubscriptionDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}
	}
}
