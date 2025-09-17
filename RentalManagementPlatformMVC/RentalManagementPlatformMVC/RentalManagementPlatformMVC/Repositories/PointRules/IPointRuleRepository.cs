using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.PointRules
{
	public interface IPointRuleRepository
	{
		// 基本 CRUD
		Task<PagedResult<PointRule>> GetPagedPointRulesAsync(int pageIndex, int pageSize);
		Task<PointRule?> GetRuleByIdAsync(int ruleId);
		Task<PointRule> CreateRuleAsync(PointRule rule);
		Task<bool> DeleteRuleAsync(int ruleId);
		Task<PointRule> EditRuleAsync(PointRule rule);
		Task<bool> UpdateRuleStatusAsync(int ruleId, bool isActive);
		Task<PointRule?> GetActiveRuleAsync();
	}
}
