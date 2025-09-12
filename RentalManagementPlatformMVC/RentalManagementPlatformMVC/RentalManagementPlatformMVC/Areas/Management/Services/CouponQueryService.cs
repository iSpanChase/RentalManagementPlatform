// 優惠券查詢服務層
// 功能: 提供「查詢資料」的商業邏輯 (模糊搜尋、分頁)
// 注意: 不直接操作 DbContext，而是透過 Repository

using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.DTOS;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;

namespace RentalManagementPlatformMVC.Areas.Management.Services;

public class CouponQueryService: ICouponQueryService
{
	private readonly ICouponReadRepository _repo;
	public CouponQueryService(ICouponReadRepository repo)
	{
		_repo = repo;
	}

	public async Task<List<CouponDto>> GetListAsync(string? keyword = null)//收尋
	{
		var query = _repo.Query();

		if (!string.IsNullOrWhiteSpace(keyword))
		{
			query = query.Where(c =>
				c.CouponName.Contains(keyword) ||
				c.DiscountCode.Contains(keyword) ||
				c.Description.Contains(keyword)
			);
		}

		return await query
			.OrderByDescending(c => c.CouponId)
			.Select(c => new CouponDto
			{
				CouponId = c.CouponId,
				CouponName = c.CouponName,
				DiscountCode = c.DiscountCode,
				Description = c.Description,
				MinRentalPeriod = c.MinRentalPeriod,
				MaxRentalPeriod = c.MaxRentalPeriod,
				StartRentalPeriod = c.StartRentalPeriod,
				EndRentalPeriod = c.EndRentalPeriod,
				DiscountMethod = c.DiscountMethod,
				DiscountQuota = c.DiscountQuota,
				LowSpend = c.LowSpend,
				EndAt = c.EndAt,
				IsDeleted = c.IsDeleted
			})
			.ToListAsync();
	}

	public async Task<(List<CouponDto> Data, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, string? keyword = null)//分頁
	{
		var query = _repo.Query();

		if (!string.IsNullOrWhiteSpace(keyword))
		{
			query = query.Where(c =>
				c.CouponName.Contains(keyword) ||
				c.DiscountCode.Contains(keyword) ||
				c.Description.Contains(keyword)
			);
		}

		var totalCount = await query.CountAsync();//取得總筆數

		var data = await query
			.OrderByDescending(c => c.CouponId)
			.Skip((pageIndex - 1) * pageSize)
			.Take(pageSize)
			.Select(c => new CouponDto
			{
				CouponId = c.CouponId,
				CouponName = c.CouponName,
				DiscountCode = c.DiscountCode,
				Description = c.Description,
				MinRentalPeriod = c.MinRentalPeriod,
				MaxRentalPeriod = c.MaxRentalPeriod,
				StartRentalPeriod = c.StartRentalPeriod,
				EndRentalPeriod = c.EndRentalPeriod,
				DiscountMethod = c.DiscountMethod,
				DiscountQuota = c.DiscountQuota,
				LowSpend = c.LowSpend,
				EndAt = c.EndAt,
				IsDeleted = c.IsDeleted
			})
			.ToListAsync();

		return (data, totalCount);
	}
}
