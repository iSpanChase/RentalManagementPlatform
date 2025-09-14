using AutoMapper;
using DocumentFormat.OpenXml.Vml.Office;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.SubscriptionPlan;
using RentalManagementPlatformMVC.Exceptions;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories.SubscriptionPlans;

namespace RentalManagementPlatformMVC.Services.SubscriptionPlans
{
	public class SubscriptionPlanService : ISubscriptionPlanService
	{
		private readonly ISubscriptionPlanRepository _subscriptionPlanRepository;
		// 簡化映射
		private readonly IMapper _mapper; 

		public SubscriptionPlanService(ISubscriptionPlanRepository subscriptionPlanRepository, IMapper mapper)
		{
			_subscriptionPlanRepository = subscriptionPlanRepository;
			_mapper = mapper;
		}

		/// <summary>
		/// 取得分頁的訂閱方案清單。
		/// </summary>
		/// <param name="pageIndex">分頁索引（從 0 開始）。</param>
		/// <param name="pageSize">每頁顯示的資料筆數。</param>
		/// <returns>分頁的訂閱方案資料集合。</returns>
		public async Task<PagedResult<SubscriptionPlanDto>> GetPagedPlansAsync(int pageIndex, int pageSize)
		{
			var pagedEntities = await _subscriptionPlanRepository.GetPagedPlansAsync(pageIndex, pageSize);

			var planDtos = _mapper.Map<List<SubscriptionPlanDto>>(pagedEntities.Items);
			
			// 設定操作權限和訂閱者數量
			foreach (var dto in planDtos)
			{
				var activeSubscriberCount = await _subscriptionPlanRepository.GetActiveSubscriberCountAsync(dto.PlanId);
				dto.SubscriberCount = activeSubscriberCount;

				// 商業邏輯：
				// - 所有方案都可以編輯（包含已啟用有訂閱者的方案）
				// - 只有未啟用且沒有任何使用者（包含歷史）的方案可以刪除
				// - 只有未啟用的方案可以啟用
				// - 所有已啟用的方案都可以停用（包含有訂閱者的方案）
				dto.CanEdit = await CanEditPlanAsync(dto.PlanId);
				dto.CanDelete = await CanDeletePlanAsync(dto.PlanId);
				dto.CanActivate = await CanActivatePlanAsync(dto.PlanId);
				dto.CanDeactivate = await CanDeactivatePlanAsync(dto.PlanId);
			}

			return new PagedResult<SubscriptionPlanDto>
			{
				Items = planDtos,
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
		}

		/// <summary>
		/// 建立新的訂閱方案。
		/// </summary>
		/// <param name="planDto">包含訂閱方案資訊的資料傳輸物件。</param>
		/// <returns>建立完成的訂閱方案資料傳輸物件。</returns>
		public async Task<SubscriptionPlanDto> CreatePlanAsync(CreatePlanDto planDto)
		{
			// 輸入驗證
			if (planDto == null)
				throw new ArgumentNullException(nameof(planDto));

			if (string.IsNullOrWhiteSpace(planDto.PlanName))
				throw new ArgumentException("方案名稱不能為空", nameof(planDto));

			if (planDto.MonthlyFee < 0)
				throw new ArgumentException("月費不能為負數", nameof(planDto));

			// 使用 AutoMapper 轉換
			var entity = _mapper.Map<SubscriptionPlan>(planDto);
			entity.IsActive = false;
			entity.CreatedAt = DateTime.UtcNow;

			// 檢查方案名稱是否重複
			var existingPlan = await _subscriptionPlanRepository.GetByNameAsync(planDto.PlanName);
			if (existingPlan != null)
				throw new PlanNameExistException("方案名稱已存在");

			var createdEntity = await _subscriptionPlanRepository.CreatePlanAsync(entity);

			return _mapper.Map<SubscriptionPlanDto>(createdEntity);
		}

		/// <summary>
		/// 刪除指定的訂閱方案。
		/// </summary>
		/// <param name="planId">訂閱方案的唯一識別碼。</param>
		/// <returns>如果刪除成功則回傳 true，否則回傳 false。</returns>
		public async Task<bool> DeletePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);

			if (plan == null)
				throw new PlanNotFoundException("找不到指定的方案");

			if (plan.IsActive)
				throw new InvalidOperationException("無法刪除啟用中的方案");

			var hasSubscribers = await _subscriptionPlanRepository.HasSubscribersAsync(planId);

			if (hasSubscribers)
				throw new PlanInUseException("無法刪除有訂閱紀錄的方案");

			return await _subscriptionPlanRepository.DeletePlanAsync(planId);
		}

		public async Task<SubscriptionPlanDto> EditPlanAsync(EditPlanDto planDto)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planDto.PlanId);

			if (plan == null)
				throw new PlanNotFoundException("找不到指定的方案");

			// 允許編輯已啟用的方案

			// 檢查方案名稱是否重複
			if (!string.Equals(plan.PlanName, planDto.PlanName, StringComparison.OrdinalIgnoreCase))
			{
				var existingPlan = await _subscriptionPlanRepository.GetByNameAsync(planDto.PlanName);
				if (existingPlan != null && existingPlan.PlanId != planDto.PlanId)
					throw new PlanNameExistException("方案名稱已存在");
			}

			// 更新欄位
			plan.PlanName = planDto.PlanName;
			plan.MonthlyFee = planDto.MonthlyFee;
			plan.CreatedAt = DateTime.UtcNow;
			plan.PerkPriority = planDto.PerkPriority;
			plan.PerkAnalytics = planDto.PerkAnalytics;
			plan.CommissionRate = planDto.CommissionRate;

			var updatedPlan = await _subscriptionPlanRepository.EditPlanAsync(plan);
			return _mapper.Map<SubscriptionPlanDto>(updatedPlan);
		}

		/// <summary>
		/// 啟用指定的訂閱方案
		/// </summary>
		public async Task<bool> ActivatePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			if (plan == null)
				throw new PlanNotFoundException("找不到指定的方案");

			if (plan.IsActive)
				throw new InvalidOperationException("方案已經是啟用狀態");

			return await _subscriptionPlanRepository.UpdatePlanStatusAsync(planId, true);
		}

		/// <summary>
		/// 停用指定的訂閱方案
		/// </summary>
		public async Task<bool> DeactivatePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			if (plan == null)
				throw new PlanNotFoundException("找不到指定的方案");

			if (!plan.IsActive)
				throw new InvalidOperationException("方案已經是停用狀態");

			// 允許停用有使用者的方案，但會提醒使用者
			return await _subscriptionPlanRepository.UpdatePlanStatusAsync(planId, false);
		}

		/// <summary>
		/// 檢查方案是否可以編輯
		/// </summary>
		public async Task<bool> CanEditPlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			// 所有方案都可以編輯，包含已啟用的方案
			return plan != null;
		}

		/// <summary>
		/// 檢查方案是否可以刪除
		/// </summary>
		public async Task<bool> CanDeletePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			if (plan == null || plan.IsActive)
				return false;

			// 檢查是否有任何訂閱紀錄（包含歷史）
			var hasSubscribers = await _subscriptionPlanRepository.HasSubscribersAsync(planId);
			return !hasSubscribers;
		}

		/// <summary>
		/// 檢查方案是否可以啟用
		/// </summary>
		public async Task<bool> CanActivatePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			// 只有未啟用的方案可以啟用
			return plan != null && !plan.IsActive;
		}

		/// <summary>
		/// 檢查方案是否可以停用
		/// </summary>
		public async Task<bool> CanDeactivatePlanAsync(int planId)
		{
			var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
			// 只要是已啟用的方案都可以停用，不管是否有使用者
			return plan != null && plan.IsActive;
		}

		/// <summary>
		/// 取得方案的啟用中訂閱者數量
		/// </summary>
		public async Task<int> GetPlanActiveSubscriberCountAsync(int planId)
		{
			return await _subscriptionPlanRepository.GetActiveSubscriberCountAsync(planId);
		}
	}
}
