using RentalManagementPlatformMVC.DTOs;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories
{
	public interface IPaymentRepository
	{
		// 初始載入：取得所有帳務資料（支援分頁）
		Task<(IEnumerable<Payment>, int)> GetPagedPaymentAsync(int pageIndex, int pageSize);

		//// 詳細頁面：根據帳務ID取得金流詳細資訊
		//Task<Payment?> GetByIdAsync(int paymentId);

		//// 動態條件查詢：根據篩選條件查詢帳務
		//Task<(IEnumerable<Payment>, int)> SearchAsync(PaymentSearchCriteriaDto criteria, int pageIndex, int pageSize);
	}
}
