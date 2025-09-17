using RentalManagementPlatformMVC.Areas.Management.DTOS;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services.Interfaces
{
	public interface ICouponQueryService
	{
		Task<(List<CouponDto> Data, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, string? keyword = null);
	}
}
