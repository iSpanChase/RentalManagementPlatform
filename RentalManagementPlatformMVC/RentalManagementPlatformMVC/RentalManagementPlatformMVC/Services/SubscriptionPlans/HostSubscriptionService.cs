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
			var (entities, totalCount) = await _hostSubscriptionRepository.GetPagedHostSubscriptionsAsync(pageIndex, pageSize);

			var hostPlanDto = _mapper.Map<List<HostSubscriptionDto>>(entities);

			return new PagedResult<HostSubscriptionDto>
			{
				Items = hostPlanDto,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}
	}
}
