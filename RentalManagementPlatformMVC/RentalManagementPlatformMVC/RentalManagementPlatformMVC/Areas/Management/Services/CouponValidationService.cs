using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.DTOS;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;

namespace RentalManagementPlatformMVC.Areas.Management.Services;

	public class CouponValidationService
	{
		private readonly ICouponReadRepository _repo;
		public CouponValidationService(ICouponReadRepository repo)
		{
			_repo = repo;
		}

	public async Task<CouponValidationResultDto> ValidateCouponAsync(string discountCode, decimal totalPrice) //驗證優惠券;已非同步方式,回傳至Task<bool>。假如驗證通過回傳true,若未通過則回傳false
	{
		var coupon = await _repo.Query().FirstOrDefaultAsync(c => c.DiscountCode == discountCode);//收尋資料庫第一筆符合的紀錄,若無則會回傳null
		
		if (coupon == null)
		//判斷優惠碼是否存在
		{
			return CouponValidationResultDto.Failure("查無此優惠券");
		}

		if (coupon.EndAt.HasValue && coupon.EndAt.Value < DateTime.UtcNow)
		//驗證優惠碼是否過期
		//由coupon.EndAt.HasValue判斷EndAt是否有結束時間的值
		//假如有值則會判斷coupon.EndAt.Value是否有小於DateTime.UtcNow(目前的時間)
		{
			return CouponValidationResultDto.Failure("優惠券已過期");
		}
		
		/*
		 * if (coupon.IsDeleted)
		//判斷優惠券是否被軟刪除
		//if (coupon.IsDeleted==true)就表示優惠券已被刪除
		{
			return CouponValidationResultDto.Failure("優惠券已失效");
		}

		if (coupon.MinRentalPeriod.HasValue && ) 
		{
			
		}
		*/

		return CouponValidationResultDto.Success();
	}
}
