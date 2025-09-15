using Microsoft.EntityFrameworkCore;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.PointRules
{
	public class PointRuleRepository : IPointRuleRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public PointRuleRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 取得分頁的點數規則清單，按建立時間降序排列。
		/// </summary>
		/// <param name="pageIndex">頁面索引，從 1 開始。</param>
		/// <param name="pageSize">每頁顯示的資料筆數。</param>
		/// <returns>回傳包含點數規則清單與分頁資訊的 PagedResult 物件。</returns>
		public async Task<PagedResult<PointRule>> GetPagedPointRulesAsync(int pageIndex, int pageSize)
		{
			var query = _context.PointRules.AsNoTracking();

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(p => p.CreatedAt)
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<PointRule>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 建立新的點數規則
		/// </summary>
		/// <param name="rule">要建立的點數規則物件</param>
		/// <returns>回傳已建立的點數規則物件，包含資料庫生成的識別碼</returns>
		public async Task<PointRule> CreateRuleAsync(PointRule rule)
		{
			_context.PointRules.Add(rule);
			await _context.SaveChangesAsync();
			return rule;
		}

		/// <summary>
		/// 刪除點數規則
		/// </summary>
		/// <param name="ruleId">要刪除的點數規則識別碼</param>
		/// <returns>回傳是否成功刪除</returns>
		public async Task<bool> DeleteRuleAsync(int ruleId)
		{
			var rule = await _context.PointRules.FindAsync(ruleId);
			if (rule == null) return false;

			_context.PointRules.Remove(rule);
			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// 編輯現有的點數規則
		/// </summary>
		/// <param name="rule">要更新的點數規則物件</param>
		/// <returns>回傳更新後的點數規則物件</returns>
		public async Task<PointRule> EditRuleAsync(PointRule rule)
		{
			_context.PointRules.Update(rule);
			await _context.SaveChangesAsync();
			return rule;
		}



		// 以下是檢查驗證用的輔助方法
		public async Task<PointRule?> GetRuleByIdAsync(int ruleId)
		{
			return await _context.PointRules
				.AsNoTracking()
				.FirstOrDefaultAsync(r => r.RuleId == ruleId);
		}

		/// <summary>
		/// 更新點數規則的啟用狀態
		/// </summary>
		/// <param name="ruleId">要更新的點數規則識別碼</param>
		/// <param name="isActive">新的啟用狀態</param>
		/// <returns>回傳是否成功更新</returns>
		public async Task<bool> UpdateRuleStatusAsync(int ruleId, bool isActive)
		{
			var rule = await _context.PointRules.FindAsync(ruleId);
			if (rule == null) return false;

			rule.IsActive = isActive;
			await _context.SaveChangesAsync();
			return true;
		}

		/// <summary>
		/// 取得目前啟用中的點數規則
		/// </summary>
		/// <returns>回傳目前啟用中的點數規則，如果沒有則回傳 null</returns>
		public async Task<PointRule?> GetActiveRuleAsync()
		{
			return await _context.PointRules
				.AsNoTracking()
				.FirstOrDefaultAsync(r => r.IsActive == true);
		}
	}
}
