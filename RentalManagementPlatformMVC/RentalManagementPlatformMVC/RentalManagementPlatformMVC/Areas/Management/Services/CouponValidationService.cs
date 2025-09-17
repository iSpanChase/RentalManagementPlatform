//優惠券商業邏輯判斷

using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformMVC.Areas.Management.DTOS;
using RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces;
using RentalManagementPlatformMVC.Models;
using System.Linq;

namespace RentalManagementPlatformMVC.Areas.Management.Services;

	public class CouponValidationService
	{
		private readonly ICouponReadRepository _repo;
		private readonly ICouponDistrictReadRepository _districtrepo;
		private readonly IBookingReadRepository _bookingrepo;
		private readonly IUserReadRepository _userrepo;

		public CouponValidationService(ICouponReadRepository repo, ICouponDistrictReadRepository districtrepo, IBookingReadRepository bookingrepo, IUserReadRepository userrepo)
		{
			_repo = repo;
			_districtrepo = districtrepo;
			_bookingrepo = bookingrepo;
			_userrepo = userrepo;
		}


	public async Task<CouponValidationResultDto> ValidateCouponAsync(
		string discountCode,
		int leaseDays = 0,
		decimal totalAmount = 0,
		int? districtId = null,
		int? userId = null,
		int? guestId = null,
		bool isNemUserOnly=false,//新用戶的參數(需要動資料庫)
		bool isBirthdayOnly= false//生日的參數(需要動資料庫)
		) //(discountCode=>優惠代碼、leaseDays=>租期天數、totalAmount=>訂單總金額、districtId=>地區優惠判斷、userId=>查詢新用戶及生日使用、guestId=>查詢新用戶使用)
	{
		var coupon = await _repo.Query().FirstOrDefaultAsync(c => c.DiscountCode == discountCode);//收尋資料庫第一筆符合的紀錄,若無則會回傳null
		
		// 1. 查詢優惠券
		if (coupon == null)
		//判斷優惠碼是否存在
		{
			return CouponValidationResultDto.Failure("查無此優惠券");
		}

		//2. 是否過期
		if (coupon.EndAt.HasValue && coupon.EndAt.Value < DateTime.UtcNow)
		//驗證優惠碼是否過期
		//由coupon.EndAt.HasValue判斷EndAt是否有結束時間的值
		//假如有值則會判斷coupon.EndAt.Value是否有小於DateTime.UtcNow(目前的時間)
		{
			return CouponValidationResultDto.Failure("優惠券已過期");
		}

		// 3. 租期區間
		if (coupon.StartRentalPeriod.HasValue && coupon.EndRentalPeriod.HasValue)
		//判斷租期時間是否在優惠券的使用範圍內
		//由coupon.StartRentalPeriod.HasValue判斷StartRentalPeriod是否有開始租期的值
		//由coupon.EndRentalPeriod.HasValue判斷EndRentalPeriod是否有結束租期的值
		//假如兩者都有值則會判斷rentalDate是否有小於coupon.StartRentalPeriod.Value(開始租期的值)或大於coupon.EndRentalPeriod.Value(結束租期的值)
		{
			return CouponValidationResultDto.Failure($"租期要在{coupon.StartRentalPeriod}到{coupon.EndRentalPeriod}區間");
		}

		// 4. 是否被刪除
		if (coupon.IsDeleted)
		//判斷優惠券是否被軟刪除
		//if (coupon.IsDeleted==true)就表示優惠券已被刪除
		{
			return CouponValidationResultDto.Failure("優惠券已失效");
		}

		// 5. 百分比金額與固定金額合理性
		if (coupon.DiscountMethod == "percentage") 
		//判斷優惠券的折扣%及固定金額是否合理
		//判斷折扣方式是否為百分比
		{
			if (coupon.DiscountQuota <= 0 || coupon.DiscountQuota > 100) 
			//判斷折扣百分比是否合理(大於0且小於等於100)
			{
				return CouponValidationResultDto.Failure("優惠券使用不合理");
			}
		}
		else if (coupon.DiscountMethod == "amount") 
		//判斷折扣方式是否為固定金額
		{
			if (coupon.DiscountQuota <= 0) 
			//判斷折扣金額是否合理(大於0)
			{
				return CouponValidationResultDto.Failure("優惠券使用不合理");
			}
		}

		// 6.最短租期
		if (coupon.MinRentalPeriod.HasValue && leaseDays < coupon.MinRentalPeriod.Value)
		//判斷租期是否符合最短租期要求
		//由coupon.MinRentalPeriod.HasValue判斷MinRentalPeriod是否有最小租期的值
		//假如有值則會判斷leaseDays是否有小於coupon.MinRentalPeriod.Value(最小租期的值)
		//leaseDays:租期天數,但不是Coupon可決定,需要從外部傳入
		//leaseDays = check_in - check_out
		{
			return CouponValidationResultDto.Failure($"租期不足{coupon.MinRentalPeriod.Value}天");
		}

		// 7. 最常租期
		if (coupon.MaxRentalPeriod.HasValue && leaseDays > coupon.MaxRentalPeriod.Value)
		//判斷租期是否符合最長租期要求
		//由coupon.MaxRentalPeriod.HasValue判斷MaxRentalPeriod是否有最大租期的值
		//假如有值則會判斷leaseDays是否有小於coupon.MaxRentalPeriod.Value(最大租期的值)
		//leaseDays:租期天數,但不是Coupon可決定,需要從外部傳入
		//leaseDays = check_in - check_out
		{
			return CouponValidationResultDto.Failure($"租期超過{coupon.MaxRentalPeriod.Value}天");
		}

		// 8. 最低金額
		if (coupon.LowSpend.HasValue && totalAmount < coupon.LowSpend.Value)
		//判斷消費金額是否符合最低消費要求
		//由coupon.LowSpend.HasValue判斷LowSpend是否有最低消費的值
		//假如有值則會判斷totalAmount是否有小於coupon.LowSpend.Value(最低消費的值)
		//totalAmount:消費金額,但不是Coupon可決定,需要從外部傳入
		//totalAmount=
		{
			return CouponValidationResultDto.Failure($"價格不可低於{coupon.LowSpend.Value}元");
		}

		// 9. 地區限制
		if (districtId.HasValue) //只有districtId有值時,才會進行判斷避免null
		{
			var districts = await  _districtrepo.Query()
				.Where(cd=>cd.CouponId == coupon.CouponId)
				.Select(cd=>cd.DistrictId)
				.ToListAsync();
			if(districts.Any() && !districts.Contains(districtId.Value)) 
			{
				return CouponValidationResultDto.Failure($"優惠券不是用於此{districtId.Value}");
			}
		}

		// 10. 新用戶
		if (userId.HasValue && isNemUserOnly) 
		//條件需要再使用者id及是新用戶的狀態才會成立
		{
			var bookingCount = await _bookingrepo.CountByUserId(userId.Value);//查詢使用者的歷史訂單數量
			if (bookingCount>0)//回傳為0=>新用戶;回傳為1=>舊用戶
			{
				return CouponValidationResultDto.Failure("此優惠券僅限新用戶使用");
			}
		}

		// 11. 生日
		if (userId.HasValue && isBirthdayOnly)
		//條件需要指定為使用者id及對應生日優惠券的狀態才會成立
		{
			var user = await _userrepo.GetByIdAsync(userId.Value);
			if (user != null)
			{
				var todayMonth = DateTime.UtcNow.Month;
				if(user.BirthDate.Month != todayMonth) 
				{
					return CouponValidationResultDto.Failure("此優惠券僅限生日當月使用");
				}
			}
			else
			{
				return CouponValidationResultDto.Failure("查無使用這資料");
			}
		}

		return CouponValidationResultDto.Success();
	}
}
