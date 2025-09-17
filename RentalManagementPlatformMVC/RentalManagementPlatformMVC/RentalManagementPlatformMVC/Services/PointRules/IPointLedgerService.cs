using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.PointRules;

namespace RentalManagementPlatformMVC.Services.PointRules
{
    public interface IPointLedgerService
    {
        // 初始載入：取得所有點數帳本資料（支援分頁）
        Task<PagedResult<PointLedgerDto>> GetPagedPointLedgersAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據點數帳本的ID取得詳細資訊
		//Task<PointLedgerDto?> GetPointLedgerByIdAsync(int ledgerId);

		// 動態條件查詢：根據篩選條件查詢點數帳本資料（支援分頁）
		Task<PagedResult<PointLedgerDto>> SearchPointLedgersAsync(PointLedgerSearchCriteriaDto criteria, int pageIndex, int pageSize);
    }
}