using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs;

namespace RentalManagementPlatformMVC.Services
{
	public interface IPaymentService
	{
		// 初始載入：取得所有帳務資料（支援分頁）
		Task<PagedResult<PaymentDto>> GetPagedPaymentsAsync(int pageIndex, int pageSize);
	}
}
