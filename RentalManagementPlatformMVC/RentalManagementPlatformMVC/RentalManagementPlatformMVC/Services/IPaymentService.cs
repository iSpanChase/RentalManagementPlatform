using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
	public interface IPaymentService
	{
		// 初始載入：取得所有帳務資料（支援分頁）
		Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據帳務ID取得金流詳細資訊
		Task<PaymentDetailDto?> GetPaymentDetailByIdAsync(int paymentId);

		// 動態條件查詢：根據篩選條件查詢訂單
		Task<PagedResult<PaymentDto>> SearchPaymentsAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
