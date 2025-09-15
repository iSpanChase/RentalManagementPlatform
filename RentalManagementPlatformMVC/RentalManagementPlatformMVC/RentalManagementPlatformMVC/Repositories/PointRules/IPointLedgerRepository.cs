using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.PointRules
{
    public interface IPointLedgerRepository
    {
		// 初始載入：取得所有PointLedger資料（支援分頁）
		Task<(IEnumerable<PointLedger>, int)> GetPagedPointLedgersAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據PointLedger的ID取得詳細資訊
		//Task<PointLedger?> GetPointLedgerDetailByIdAsync(int ledgerId);

		// 動態條件查詢：根據篩選條件查詢PointLedger資料（支援分頁）
		Task<(IEnumerable<PointLedger>, int)> SearchPointLedgersAsync(PointLedgerSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}