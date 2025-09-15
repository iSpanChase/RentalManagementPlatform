using Microsoft.EntityFrameworkCore;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Repositories.SubscriptionPlans
{
	public class SubscriptionPlanRepository : ISubscriptionPlanRepository
	{
		private readonly RentalManagementPlatformSqlContext _context;

		public SubscriptionPlanRepository(RentalManagementPlatformSqlContext context)
		{
			_context = context;
		}

		/// <summary>
		/// 取得分頁的訂閱方案清單。
		/// </summary>
		/// <param name="pageIndex">目前頁碼（從 1 開始）。</param>
		/// <param name="pageSize">每頁顯示的資料筆數。</param>
		/// <returns>分頁結果，包含訂閱方案清單及分頁資訊。</returns>
		public async Task<PagedResult<SubscriptionPlan>> GetPagedPlansAsync(int pageIndex, int pageSize)
		{
			var query = _context.SubscriptionPlans.AsNoTracking();

			var totalCount = await query.CountAsync();

			var items = await query
				.OrderByDescending(p => p.CreatedAt)
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.ToListAsync();

			return new PagedResult<SubscriptionPlan>
			{
				Items = items,
				PageIndex = pageIndex,
				PageSize = pageSize,
				TotalCount = totalCount
			};
		}

		/// <summary>
		/// 建立新的訂閱方案。
		/// </summary>
		/// <param name="plan">要新增的訂閱方案物件。</param>
		/// <returns>已建立的訂閱方案物件。</returns>
		public async Task<SubscriptionPlan> CreatePlanAsync(SubscriptionPlan plan)
		{
			_context.SubscriptionPlans.Add(plan);

			await _context.SaveChangesAsync();

			return plan;
		}

		/// <summary>
		/// 刪除指定的訂閱方案。
		/// </summary>
		/// <param name="planId">訂閱方案的唯一識別碼。</param>
		/// <returns>若刪除成功則回傳 true，否則回傳 false。</returns>
		public async Task<bool> DeletePlanAsync(int planId)
		{
			var plan = await _context.SubscriptionPlans.FindAsync(planId);

			if (plan == null) return false;

			_context.SubscriptionPlans.Remove(plan);

			await _context.SaveChangesAsync();

			return true;
		}

		/// <summary>
		/// 更新指定的訂閱方案資訊。
		/// </summary>
		/// <param name="plan">包含更新資料的訂閱方案物件。</param>
		/// <returns>已更新的訂閱方案物件。</returns>
		public async Task<SubscriptionPlan> EditPlanAsync(SubscriptionPlan plan)
		{
			_context.SubscriptionPlans.Update(plan);
			await _context.SaveChangesAsync();
			return plan;
		}



		// 以下是檢查驗證用的輔助方法

		// 根據方案名稱取得方案
		public async Task<SubscriptionPlan?> GetByNameAsync(string planName)
		{
			return await _context.SubscriptionPlans
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PlanName == planName);
		}

		// 根據方案 ID 取得方案
		public async Task<SubscriptionPlan?> GetPlanByIdAsync(int planId)
		{
			return await _context.SubscriptionPlans
				.AsNoTracking()
				.FirstOrDefaultAsync(p => p.PlanId == planId);
		}

		// 檢查指定方案是否有任何訂閱者（不論狀態）
		public async Task<bool> HasSubscribersAsync(int planId)
		{
			return await _context.HostSubscriptions
				.AnyAsync(hs => hs.PlanId == planId);
		}

		// 檢查指定方案是否有任何啟用中的訂閱者
		public async Task<bool> HasActiveSubscribersAsync(int planId)
		{
			return await _context.HostSubscriptions
				.AnyAsync(hs => hs.PlanId == planId && hs.Status == "active");
		}

		// 取得指定方案的啟用中訂閱者數量
		public async Task<int> GetActiveSubscriberCountAsync(int planId)
		{
			return await _context.HostSubscriptions
				.CountAsync(hs => hs.PlanId == planId && hs.Status == "active");
		}

		// 更新方案啟用狀態
		public async Task<bool> UpdatePlanStatusAsync(int planId, bool isActive)
		{
			var plan = await _context.SubscriptionPlans.FindAsync(planId);
			if (plan == null) return false;

			plan.IsActive = isActive;
			await _context.SaveChangesAsync();
			return true;
		}
	}
}
