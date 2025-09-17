using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.Payments;
using RentalManagementPlatformMVC.Repositories.Payments;

namespace RentalManagementPlatformMVC.Services.Payments
{
	public class HostPayoutService : IHostPayoutService
	{
		private readonly IHostPayoutRepository _hostPayoutRepository;

		public HostPayoutService(IHostPayoutRepository hostPayoutRepository)
		{
			_hostPayoutRepository = hostPayoutRepository;
		}

		/// <summary>
		/// 取得分頁的主機收款資料列表
		/// </summary>
		/// <param name="pageIndex">頁面索引，從0開始</param>
		/// <param name="pageSize">每頁顯示的資料筆數</param>
		/// <returns>包含主機收款資料列表和分頁資訊的分頁結果物件</returns>
		public async Task<PagedResult<HostPayoutDto>> GetPagedHostPayoutsAsync(int pageIndex, int pageSize)
		{
			var pagedEntities = await _hostPayoutRepository.GetPagedHostPayoutsAsync(pageIndex, pageSize);

			var hostPayoutDtos = pagedEntities.Items.Select(p => new HostPayoutDto
			{
				PayoutId = p.PayoutId,
				HostId = p.HostId,
				HostName = p.Host?.Name,
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
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
		}

		/// <summary>
		/// 根據主機收款ID取得詳細資訊，包含所有關聯的收款項目
		/// </summary>
		/// <param name="hostPayoutId">主機收款的唯一識別碼</param>
		/// <returns>包含詳細資訊和相關收款項目的主機收款詳細資料物件，若找不到則回傳null</returns>
		public async Task<HostPayoutDetailDto?> GetHostPayoutByIdAsync(int hostPayoutId)
		{
			var hostPayout = await _hostPayoutRepository.GetHostPayoutDetailByIdAsync(hostPayoutId);
			if (hostPayout == null) return null;

			return new HostPayoutDetailDto
			{
				HostId = hostPayout.HostId,
				HostName = hostPayout.Host?.Name,
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

		/// <summary>
		/// 根據搜尋條件動態查詢主機收款資料，支援分頁功能
		/// </summary>
		/// <param name="criteria">搜尋條件物件，包含各種篩選條件如主機ID、主機名稱、週期日期、付款日期、金額範圍、狀態等</param>
		/// <param name="pageIndex">頁面索引，從0開始</param>
		/// <param name="pageSize">每頁顯示的資料筆數</param>
		/// <returns>包含符合搜尋條件的主機收款資料列表和分頁資訊的分頁結果物件</returns>
		public async Task<PagedResult<HostPayoutDto>> SearchHostPayoutsAsync(HostSearchCriteriaDto criteria, int pageIndex, int pageSize)
		{
			// tuple swap防止輸入值顛倒
			if (criteria.MinAmount.HasValue && criteria.MaxAmount.HasValue &&
				criteria.MinAmount > criteria.MaxAmount)
			{
				(criteria.MinAmount, criteria.MaxAmount) = (criteria.MaxAmount, criteria.MinAmount);
			}

			if (criteria.PaidStartDate.HasValue && criteria.PaidEndDate.HasValue &&
				criteria.PaidStartDate > criteria.PaidEndDate)
			{
				(criteria.PaidStartDate, criteria.PaidEndDate) = (criteria.PaidEndDate, criteria.PaidStartDate);
			}

			// 呼叫 Repository 動態查詢
			var (entities, totalCount) = await _hostPayoutRepository.SearchHostPayoutsAsync(criteria, pageIndex, pageSize);

			// 映射成列表用 DTO
			var hostPayoutDtos = entities.Select(p => new HostPayoutDto
			{
				PayoutId = p.PayoutId,
				HostId = p.HostId,
				HostName = p.Host?.Name,
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
	}
}
