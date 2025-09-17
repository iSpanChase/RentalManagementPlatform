using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.Payments;

namespace RentalManagementPlatformMVC.Services.Payments
{
	public interface IPaymentService
	{
		// 初始載入：取得所有Payment資料（支援分頁）
		Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據Payment的ID取得詳細資訊
		Task<PaymentDetailDto?> GetPaymentDetailByIdAsync(int paymentId);

		// 動態條件查詢：根據篩選條件查詢Payment資料（支援分頁）
		Task<PagedResult<PaymentDto>> SearchPaymentsAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
