using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlans;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public interface IHostSubscriptionRepository
	{
		// 初始載入：取得所有HostSubscription資料（支援分頁）
		Task<PagedResult<HostSubscription>> GetPagedHostSubscriptionsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據HostSubscription的ID取得詳細資訊
		Task<HostSubscription?> GetHostSubscriptionDetailByIdAsync(int hostSubscriptionId);

		// 動態條件查詢：根據篩選條件查詢HostSubscription資料（支援分頁）
		Task<(IEnumerable<HostSubscription>, int)> SearchHostSubscriptionsAsync(HostSubscriptionSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
