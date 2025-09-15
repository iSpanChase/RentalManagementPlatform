using AutoMapper;
using RentalManagementPlatform.Common.Pagination;
using RentalManagementPlatformMVC.DTOs.PointRules;
using RentalManagementPlatformMVC.Exceptions;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformMVC.Repositories.PointRules;

namespace RentalManagementPlatformMVC.Services.PointRules
{
	public class PointRuleService : IPointRuleService
	{
		private readonly IPointRuleRepository _pointRuleRepository;
		private readonly IMapper _mapper;

		public PointRuleService(IPointRuleRepository pointRuleRepository, IMapper mapper)
		{
			_pointRuleRepository = pointRuleRepository;
			_mapper = mapper;
		}

		/// <summary>
		/// 取得分頁的點數規則資料。
		/// </summary>
		/// <param name="pageIndex">頁面索引，從 1 開始。</param>
		/// <param name="pageSize">每頁顯示的資料筆數。</param>
		/// <returns>包含點數規則資料和分頁資訊的分頁結果物件。</returns>
		public async Task<PagedResult<PointRuleDto>> GetPagedPointRulesAsync(int pageIndex, int pageSize)
		{
			var pagedEntities = await _pointRuleRepository.GetPagedPointRulesAsync(pageIndex, pageSize);

			var pointRuleDtos = _mapper.Map<List<PointRuleDto>>(pagedEntities.Items);

			foreach (var dto in pointRuleDtos)
			{
				// 商業邏輯：
				// - 所有方案都可以編輯（包含已啟用的方案）
				// - 只有未啟用且沒有啟用過的方案可以刪除
				// - 只有未啟用的方案可以啟用
				// - 所有已啟用的方案都可以停用
				dto.CanEdit = await CanEditRuleAsync(dto.RuleId);
				dto.CanDelete = await CanDeleteRuleAsync(dto.RuleId);
				dto.CanActivate = await CanActivateRuleAsync(dto.RuleId);
				dto.CanDeactivate = await CanDeactivateRuleAsync(dto.RuleId);
			}

			return new PagedResult<PointRuleDto>
			{
				Items = pointRuleDtos,
				PageIndex = pagedEntities.PageIndex,
				PageSize = pagedEntities.PageSize,
				TotalCount = pagedEntities.TotalCount
			};
		}

		/// <summary>
		/// 建立新的點數規則。
		/// </summary>
		/// <param name="createDto">包含點數規則資訊的資料傳輸物件。</param>
		/// <returns>建立完成的點數規則資料傳輸物件。</returns>
		public async Task<PointRuleDto> CreatePointRuleAsync(CreatePointRuleDto createDto)
		{
			// 輸入驗證
			if (createDto == null)
				throw new ArgumentNullException(nameof(createDto));

			if (createDto.EarnRatePerNtd <= 0)
				throw new InvalidPointRuleDataException("點數獲得必須大於0");

			if (createDto.RedeemRateNtdPerPt <= 0)
				throw new InvalidPointRuleDataException("點數使用必須大於0");

			if (createDto.MaxPointsPerOrder.HasValue && createDto.MaxPointsPerOrder < 0)
				throw new InvalidPointRuleDataException("每筆訂單回饋點數上限不能為負數");

			if (createDto.ExpiryMonths.HasValue && (createDto.ExpiryMonths < 1 || createDto.ExpiryMonths > 120))
				throw new InvalidPointRuleDataException("點數有效期限必須在1-120個月之間");

			// 日期驗證
			if (createDto.ActiveFrom.HasValue && createDto.ActiveTo.HasValue)
			{
				if (createDto.ActiveFrom >= createDto.ActiveTo)
					throw new PointRuleDateRangeException("結束日期必須晚於開始日期");
			}

			// 使用 AutoMapper 轉換
			var entity = _mapper.Map<PointRule>(createDto);
			entity.IsActive = false; 
			entity.CreatedAt = DateTime.UtcNow;

			var createdEntity = await _pointRuleRepository.CreateRuleAsync(entity);

			// 轉換為 DTO 並設定商業邏輯屬性
			var resultDto = _mapper.Map<PointRuleDto>(createdEntity);

			return resultDto;
		}

		/// <summary>
		/// 刪除點數規則。
		/// </summary>
		/// <param name="ruleId">要刪除的點數規則ID。</param>
		/// <returns>是否成功刪除。</returns>
		public async Task<bool> DeletePointRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			if (rule == null)
				throw new PointRuleNotFoundException($"找不到 ID 為 {ruleId} 的點數規則");

			// 商業邏輯驗證：已啟用的規則不能刪除
			if (rule.IsActive == true)
				throw new InvalidOperationException("無法刪除啟用中的點數規則");

			// 商業邏輯驗證：已啟用過的規則不能刪除（根據ActiveFrom是否已開始生效判斷）
			var currentTime = DateTime.UtcNow;
			if (rule.ActiveFrom.HasValue && rule.ActiveFrom <= currentTime)
				throw new PointRuleInUseException("無法刪除已啟用過的點數規則");

			return await _pointRuleRepository.DeleteRuleAsync(ruleId);
		}

		/// <summary>
		/// 編輯點數規則。
		/// </summary>
		/// <param name="editDto">包含編輯資訊的資料傳輸物件。</param>
		/// <returns>編輯完成的點數規則資料傳輸物件。</returns>
		public async Task<PointRuleDto> EditPointRuleAsync(EditPointRuleDto editDto)
		{
			if (editDto == null)
				throw new ArgumentNullException(nameof(editDto));

			// 檢查規則是否存在
			var existingRule = await _pointRuleRepository.GetRuleByIdAsync(editDto.RuleId);
			if (existingRule == null)
				throw new PointRuleNotFoundException($"找不到 ID 為 {editDto.RuleId} 的點數規則");

			// 輸入驗證
			if (editDto.EarnRatePerNtd <= 0)
				throw new InvalidPointRuleDataException("點數賺取比例必須大於0");

			if (editDto.RedeemRateNtdPerPt <= 0)
				throw new InvalidPointRuleDataException("點數兌換比例必須大於0");

			if (editDto.MaxPointsPerOrder.HasValue && editDto.MaxPointsPerOrder < 0)
				throw new InvalidPointRuleDataException("單筆訂單點數上限不能為負數");

			if (editDto.ExpiryMonths.HasValue && (editDto.ExpiryMonths < 1 || editDto.ExpiryMonths > 120))
				throw new InvalidPointRuleDataException("點數有效期限必須在1-120個月之間");

			// 日期驗證
			if (editDto.ActiveFrom.HasValue && editDto.ActiveTo.HasValue)
			{
				if (editDto.ActiveFrom >= editDto.ActiveTo)
					throw new PointRuleDateRangeException("結束日期必須晚於開始日期");
			}

			// 更新現有實體的屬性
			existingRule.EarnRatePerNtd = editDto.EarnRatePerNtd;
			existingRule.MaxPointsPerOrder = editDto.MaxPointsPerOrder;
			existingRule.ExpiryMonths = editDto.ExpiryMonths;
			existingRule.RedeemRateNtdPerPt = editDto.RedeemRateNtdPerPt;
			existingRule.ActiveFrom = editDto.ActiveFrom;
			existingRule.ActiveTo = editDto.ActiveTo;

			var updatedEntity = await _pointRuleRepository.EditRuleAsync(existingRule);
			return _mapper.Map<PointRuleDto>(updatedEntity);
		}

		/// <summary>
		/// 啟用點數規則。一次只能有一個規則啟用。
		/// </summary>
		/// <param name="ruleId">要啟用的點數規則ID。</param>
		/// <returns>是否成功啟用。</returns>
		public async Task<bool> ActivatePointRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			if (rule == null)
				throw new PointRuleNotFoundException($"找不到 ID 為 {ruleId} 的點數規則");

			// 商業邏輯驗證：只有未啟用的規則才能啟用
			if (rule.IsActive == true)
				throw new InvalidOperationException("點數規則已經是啟用狀態");

			// 商業邏輯驗證：一次只能啟用一組方案
			var activeRule = await _pointRuleRepository.GetActiveRuleAsync();
			if (activeRule != null)
			{
				// 先停用現有的活躍規則
				await _pointRuleRepository.UpdateRuleStatusAsync(activeRule.RuleId, false);
			}

			// 啟用指定的規則，並設置啟用時間（如果尚未設置）
			rule.IsActive = true;
			if (!rule.ActiveFrom.HasValue)
			{
				rule.ActiveFrom = DateTime.UtcNow;
			}

			await _pointRuleRepository.EditRuleAsync(rule);
			return true;
		}

		/// <summary>
		/// 停用點數規則。
		/// </summary>
		/// <param name="ruleId">要停用的點數規則ID。</param>
		/// <returns>是否成功停用。</returns>
		public async Task<bool> DeactivatePointRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			if (rule == null)
				throw new PointRuleNotFoundException($"找不到 ID 為 {ruleId} 的點數規則");

			// 商業邏輯驗證：只有已啟用的規則才能停用
			if (rule.IsActive != true)
				throw new InvalidOperationException("點數規則已經是停用狀態");

			return await _pointRuleRepository.UpdateRuleStatusAsync(ruleId, false);
		}



		// 以下為方案狀態變更和商業邏輯驗證
		/// <summary>
		/// 檢查點數規則是否可以編輯
		/// </summary>
		public async Task<bool> CanEditRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			// 所有規則都可以編輯，包含已啟用的規則
			return rule != null;
		}

		/// <summary>
		/// 檢查點數規則是否可以刪除
		/// </summary>
		public async Task<bool> CanDeleteRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			if (rule == null || rule.IsActive == true)
				return false;

			// 商業邏輯：如果規則曾經啟用過（即有ActiveFrom日期且已經開始生效），就不能刪除
			var currentTime = DateTime.UtcNow;
			if (rule.ActiveFrom.HasValue && rule.ActiveFrom <= currentTime)
				return false;

			return true;
		}

		/// <summary>
		/// 檢查點數規則是否可以啟用
		/// </summary>
		public async Task<bool> CanActivateRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			// 只有未啟用的規則可以啟用
			return rule != null && rule.IsActive != true;
		}

		/// <summary>
		/// 檢查點數規則是否可以停用
		/// </summary>
		public async Task<bool> CanDeactivateRuleAsync(int ruleId)
		{
			var rule = await _pointRuleRepository.GetRuleByIdAsync(ruleId);
			// 只要是已啟用的規則都可以停用
			return rule != null && rule.IsActive == true;
		}
	}
}
