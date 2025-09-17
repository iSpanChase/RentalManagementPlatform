using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.DTOS;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using RentalManagementPlatformMVC.Areas.Management.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace RentalManagementPlatformMVC.Areas.Management.Services
{
	public class CouponGuestQueryService : ICouponGuestQueryService
	{
		//注入Repository層中CouponGuest、CouponRead、UserRead的查詢
		public readonly ICouponGuestRepository _gepo;
		public readonly ICouponReadRepository _couponRepo;
		public readonly IUserReadRepository _userRepo;
		public CouponGuestQueryService(ICouponGuestRepository gepo, ICouponReadRepository couponRepo, IUserReadRepository userRepo)
		{
			_gepo = gepo;
			_couponRepo = couponRepo;
			_userRepo = userRepo;
		}

		public async Task<(List<CouponGuestDto> Data, int TotalCount)> GetPagedListAsync(int pageIndex, int pageSize, string? keyword = null)
		{
			var query = from cg in _gepo.Query()
						join c in _couponRepo.Query() on cg.CouponId equals c.CouponId into gj1
						from coupon in gj1.DefaultIfEmpty()
						join u in _userRepo.Query() on cg.GuestId equals u.UserId into gj2
						from user in gj2.DefaultIfEmpty()
						select new CouponGuestDto
						{
							CouponGuestId = cg.CouponGuestId,
							CouponId = cg.CouponId,
							GuestId = cg.GuestId,
							CreateAt = cg.CreateAt,
							RemoveAt = cg.RemoveAt,
							CouponName = coupon.CouponName,
							DiscountCode = coupon.DiscountCode,
							Name = user.Name
						};

			if (!string.IsNullOrWhiteSpace(keyword)) 
				{
					query= query.Where(x
						=>x.CouponName.Contains(keyword)||
							x.Name.Contains(keyword)||
							x.DiscountCode.Contains(keyword)
					);
				}

			var totalCount = await query.CountAsync();//取得總筆數

			//分頁
			var data = await query
				.OrderBy(x => x.CouponGuestId)
				.Skip((pageIndex - 1) * pageSize)
				.Take(pageSize)
				.Select(x =>new CouponGuestDto
				{
				CouponGuestId=x.CouponGuestId,
				CouponId=x.CouponId,
				GuestId=x.GuestId,
				CouponName=x.CouponName,
				DiscountCode=x.DiscountCode,
				Name=x.Name,
				CreateAt=x.CreateAt,
				RemoveAt=x.RemoveAt,
				})
				.ToListAsync();

			return (data, totalCount);
		}
	}
}
