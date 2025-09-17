using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Services.SubscriptionPlans
{
	public interface IHostSubscriptionService
	{
		// 初始載入：取得所有HostPayout資料（支援分頁）
		Task<PagedResult<HostSubscriptionDto>> GetPagedHostSubscriptionsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據HostPayout的ID取得詳細資訊
		Task<HostSubscriptionDetailDto?> GetHostSubscriptionByIdAsync(int hostSubscriptionId);

		// 動態條件查詢：根據篩選條件查詢HostSubscription資料（支援分頁）
		Task<PagedResult<HostSubscriptionDto>> SearchHostSubscriptionsAsync(HostSubscriptionSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
