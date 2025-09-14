using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public interface ISubscriptionPlanRepository
	{
		// 基本 CRUD
		Task<PagedResult<SubscriptionPlan>> GetPagedPlansAsync(int pageIndex, int pageSize);
		Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);
		Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan);
		Task<SubscriptionPlan?> GetByNameAsync(string planName);
		Task<bool> DeletePlanAsync(int planId);
		Task<SubscriptionPlan> EditPlanAsync(SubscriptionPlan plan);
		Task<bool> UpdatePlanStatusAsync(int planId, bool isActive);

		// 計數方法
		Task<int> GetActiveSubscriberCountAsync(int planId);

		// 檢查方法
		Task<bool> HasActiveSubscribersAsync(int planId);
		Task<bool> HasSubscribersAsync(int planId);
	}
}
