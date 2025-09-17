using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Services.PointRules
{
	public interface IPointRuleService
	{
		// 查詢相關
		Task<PagedResult<PointRuleDto>> GetPagedPointRulesAsync(int pageIndex, int pageSize);

		// CRUD 操作
		Task<PointRuleDto> CreatePointRuleAsync(CreatePointRuleDto pointRuleDto);
		Task<PointRuleDto> EditPointRuleAsync(EditPointRuleDto editDto);
		Task<bool> DeletePointRuleAsync(int ruleId);
		Task<bool> ActivatePointRuleAsync(int ruleId);
		Task<bool> DeactivatePointRuleAsync(int ruleId);

		// 商業邏輯驗證
		Task<bool> CanEditRuleAsync(int ruleId);
		Task<bool> CanDeleteRuleAsync(int ruleId);
		Task<bool> CanActivateRuleAsync(int ruleId);
		Task<bool> CanDeactivateRuleAsync(int ruleId);
	}
}
