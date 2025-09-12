using Microsoft.AspNetCore.Mvc.RazorPages;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;

namespace RentalManagementPlatformMVC.Services.SubscriptionPlans
{
	public interface ISubscriptionPlanService
	{
		// 查詢相關
		Task<PagedResult<SubscriptionPlanDto>> GetPagedPlansAsync(int pageIndex, int pageSize);
		//Task<SubscriptionPlanDto?> GetPlanByIdAsync(int planId);

		// CRUD 操作
		Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto dto);
		Task<bool> DeletePlanAsync(int planId);
		//Task<SubscriptionPlanDto> UpdatePlanAsync(int planId, UpdatePlanDto dto);
		//Task<bool> ActivatePlanAsync(int planId);
		//Task<bool> DeactivatePlanAsync(int planId);

		// 商業邏輯驗證
		//Task<bool> CanEditPlanAsync(int planId);
		//Task<bool> CanDeletePlanAsync(int planId);
		//Task<bool> CanActivatePlanAsync(int planId);
		//Task<bool> CanDeactivatePlanAsync(int planId);

		// 額外實用方法
		//Task<bool> ValidatePlanDataAsync(CreatePlanDto dto, int? excludePlanId = null);
		//Task<IEnumerable<SubscriptionPlanDto>> GetActivePlansAsync();
		//Task<int> GetPlanSubscriberCountAsync(int planId);
	}
}
