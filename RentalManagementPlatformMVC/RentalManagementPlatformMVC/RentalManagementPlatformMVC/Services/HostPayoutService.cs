using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Repositories;

namespace RentalManagementPlatformMVC.Services
{
	public class HostPayoutService : IHostPayoutService
	{
		private readonly IHostPayoutRepository _hostPayoutRepository;

		public HostPayoutService(IHostPayoutRepository hostPayoutRepository)
		{
			_hostPayoutRepository = hostPayoutRepository;
		}

		public async Task<PagedResult<HostPayoutDto>> GetPagedHostPayoutsAsync(int pageIndex, int pageSize)
		{
			var (entities, totalCount) = await _hostPayoutRepository.GetPagedHostPayoutsAsync(pageIndex, pageSize);

			var hostPayoutDtos = entities.Select(p => new HostPayoutDto
			{
				PayoutId = p.PayoutId,
				HostId = p.HostId,
				//HostName = p.Host.Name,
				CycleStart = p.CycleStart,
				CycleEnd = p.CycleEnd,
				AmountGross = p.AmountGross,
				PlatformFee = p.PlatformFee,
				AmountNet = p.AmountNet,
				PaidAt = p.PaidAt,
				Status = p.Status,
				CreatedAt = p.CreatedAt
			}).ToList();

			return new PagedResult<HostPayoutDto>
			{
				Items = hostPayoutDtos,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		public async Task<HostPayoutDetailDto?> GetHostPayoutByIdAsync(int hostPayoutId)
		{
			var hostPayout = await _hostPayoutRepository.GetHostPayoutDetailByIdAsync(hostPayoutId);
			if (hostPayout == null) return null;

			return new HostPayoutDetailDto
			{
				HostId = hostPayout.HostId,
				//HostName = hostPayout.Host?.Name,
				PayoutId = hostPayout.PayoutId,
				CycleStart = hostPayout.CycleStart,
				CycleEnd = hostPayout.CycleEnd,
				AmountGross = hostPayout.AmountGross,
				PlatformFee = hostPayout.PlatformFee,
				AmountNet = hostPayout.AmountNet,
				PaidAt = hostPayout.PaidAt,
				Status = hostPayout.Status,
				CreatedAt = hostPayout.CreatedAt,
				Items = hostPayout.HostPayoutItems?.Select(item => new HostPayoutItemDto
				{
					PayoutItemId = item.PayoutItemId,
					PayoutId = item.PayoutId,
					BookingId = item.BookingId,
					OrderNumberSnapshot = item.OrderNumberSnapshot,
					AmountGross = item.AmountGross,
					CommissionPct = item.CommissionPct,
					PlatformFee = item.PlatformFee,
					Amount = item.AmountNet
				}).ToList() ?? new List<HostPayoutItemDto>()
			};
		}
	}
}
