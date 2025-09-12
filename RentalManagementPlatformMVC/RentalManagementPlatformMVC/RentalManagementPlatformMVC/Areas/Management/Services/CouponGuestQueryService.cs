using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.DTOS;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class CouponGuestQueryService: ICouponGuestQueryService
	{
		public readonly ICouponGuestRepository _gepo;
		public CouponGuestQueryService(ICouponGuestRepository gepo)
		{
			_gepo = gepo;
		}

		public Task<List<CouponGuestDto>> GetListAsync(string? keyword = null)
		{
			throw new NotImplementedException();
		}
	}
}
