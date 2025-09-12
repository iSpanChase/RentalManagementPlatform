using RentalManagementPlatformMVC.Areas.Management.DTOS;

namespace RentalManagementPlatformMVC.Areas.Management.Services.Interfaces
{
	public interface ICouponGuestQueryService
	{
		Task<List<CouponGuestDto>> GetListAsync(string? keyword = null);
	}
}
