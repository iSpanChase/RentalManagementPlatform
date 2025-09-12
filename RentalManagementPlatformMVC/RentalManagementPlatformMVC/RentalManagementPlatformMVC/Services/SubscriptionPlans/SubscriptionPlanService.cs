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
			
			// 設定操作權限
			foreach (var dto in planDtos)
			{
				dto.CanEdit = !dto.IsActive;
				dto.CanDelete = !dto.IsActive;
				dto.CanActivate = !dto.IsActive;
				dto.CanDeactivate = dto.IsActive;
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




		//private async Task<bool> CanEditPlanAsync(int planId)
		//{
		//	var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
		//	if (plan == null || plan.IsActive)
		//		return false;

		//	// 檢查是否有房東正在使用此方案
		//	var hasActiveSubscribers = await _subscriptionPlanRepository.HasActiveSubscribersAsync(planId);
		//	return !hasActiveSubscribers;
		//}

		//private async Task<bool> CanDeletePlanAsync(int planId)
		//{
		//	var plan = await _subscriptionPlanRepository.GetPlanByIdAsync(planId);
		//	if (plan == null || plan.IsActive)
		//		return false;

		//	// 檢查是否有房東正在使用此方案（包含歷史訂閱）
		//	var hasSubscribers = await _subscriptionPlanRepository.HasSubscribersAsync(planId);
		//	return !hasSubscribers;
		//}
	}
}
