using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public interface ISubscriptionPlanRepository
	{
		// 基本 CRUD
		Task<PagedResult<SubscriptionPlan>> GetPagedPlansAsync(int pageIndex, int pageSize);
		//Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);
		Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan);
		Task<SubscriptionPlan?> GetByNameAsync(string planName);
		Task<bool> DeletePlanAsync(int planId);
		//Task<bool> UpdatePlanAsync(SubscriptionPlan plan);
		//Task<bool> DeactivatePlanAsync(int planId);



		// 計數方法
		//Task<int> GetActivePlanCountAsync();
		//Task<int> GetSubscriberCountAsync(int planId);

		// 檢查方法
		//Task<bool> HasActiveSubscribersAsync(int planId);
		Task<bool> HasSubscribersAsync(int planId);
		Task<SubscriptionPlan?> GetPlanByIdAsync(int planId);
	}
}
