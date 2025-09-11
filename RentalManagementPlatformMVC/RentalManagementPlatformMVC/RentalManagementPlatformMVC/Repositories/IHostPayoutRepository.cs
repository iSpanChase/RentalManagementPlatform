using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public interface IHostPayoutRepository
	{
		// 初始載入：取得所有HostPayout資料（支援分頁）
		Task<(IEnumerable<HostPayout>, int)> GetPagedHostPayoutsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據HostPayout的ID取得詳細資訊
		Task<HostPayout?> GetHostPayoutDetailByIdAsync(int hostPayoutId);

		// 動態條件查詢：根據篩選條件查詢HostPayout資料（支援分頁）
		Task<(IEnumerable<HostPayout>, int)> SearchHostPayoutsAsync(HostSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
