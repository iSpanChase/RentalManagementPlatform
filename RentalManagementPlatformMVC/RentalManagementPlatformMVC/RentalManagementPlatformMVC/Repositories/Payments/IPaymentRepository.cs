using RentalManagementPlatformMVC.DTOs.Payments;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.Payments
{
	public interface IPaymentRepository
	{
		// 初始載入：取得所有Payment資料（支援分頁）
		Task<(IEnumerable<Payment>, int)> GetPagedPaymentsAsync(int pageIndex, int pageSize);

		// 詳細頁面：根據Payment的ID取得詳細資訊
		Task<Payment?> GetPaymentDetailByIdAsync(int paymentId);

		// 動態條件查詢：根據篩選條件查詢Payment資料（支援分頁）
		Task<(IEnumerable<Payment>, int)> SearchPaymentsAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
