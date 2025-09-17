using RentalManagementPlatformMVC.Areas.Management.DTOS;

namespace RentalManagementPlatformMVC.Areas.Management.Services.Interfaces
{
	public interface ICouponGuestQueryService
	{
		//回傳dto列表與總筆數
		Task<(List<CouponGuestDto> Data, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, string? keyword = null);
	}
}
