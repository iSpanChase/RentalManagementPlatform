//優惠券商業邏輯判斷(由CouponApiService使用於優惠券驗證)

using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using RentalManagementPlatformMVC.Models;
using RentalManagementPlatformWebAPI.DTOS;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repository.Interfaces;
using System.Linq;

namespace RentalManagementPlatformWebAPI.Services;

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
		int? cityId = null,//地區限制的參數(需要動資料庫,做屬性的自動判斷)
		int? userId = null,
		//int? guestId = null,
		//bool isNewUserOnly = false,//新用戶的參數(需要動資料庫,做屬性的自動判斷)
		//bool isBirthdayOnly = false,//生日的參數(需要動資料庫,做屬性的自動判斷)
		DateTime? useDate = null) //(discountCode=>優惠代碼、leaseDays=>租期天數、totalAmount=>訂單總金額、districtId=>地區優惠判斷、userId=>查詢新用戶及生日使用、guestId=>查詢新用戶使用)
	{
		var coupon = await _repo.Query().FirstOrDefaultAsync(c => c.DiscountCode == discountCode);//收尋資料庫第一筆符合的紀錄,若無則會回傳null
		
		//☆基本條件
		var basicResult = ValidateBasic(coupon, leaseDays, totalAmount);
		if (!basicResult.IsValid) return basicResult;

		//☆租期與使用日期
		var dateResult = ValidateDate(coupon, useDate, leaseDays);
		if (!dateResult.IsValid) return dateResult;

		//☆地區限制
		var districtResult = await ValidateDistrictAsync(coupon, cityId);
		if (!districtResult.IsValid) return districtResult;

		//判斷新用戶及生日優惠,自動判斷優惠券的類型(但是作為寫死的方式)
		bool isNewUserOnly = discountCode.StartsWith("WELCOME");
		bool isBirthdayOnly = discountCode.StartsWith("BDAY");

		//☆新用戶
		if (isNewUserOnly && userId.HasValue) 
		{
			var newUserResult = await ValidateNewUserAsync(coupon, userId.Value);
			if (!newUserResult.IsValid) return newUserResult;
		}

		//☆生日優惠
		if (isBirthdayOnly && userId.HasValue) 
		{
			var birthdayResult = await ValidateBirthdayAsync(coupon, userId.Value);
			if (!birthdayResult.IsValid) return birthdayResult;
		}
		return CouponValidationResultDto.Success("恭喜啊!優惠券驗證成功");
	}
		
		private CouponValidationResultDto ValidateBasic(Coupon coupon, int leaseDays, decimal totalAmount)
	{
		// ☆查詢優惠券
		if (coupon == null)
			//判斷優惠碼是否存在
		{
			return CouponValidationResultDto.Failure("查無此優惠券");
		}
		// ☆是否過期
		if (coupon.EndAt.HasValue && coupon.EndAt.Value < DateTime.UtcNow)
			//驗證優惠碼是否過期
			//由coupon.EndAt.HasValue判斷EndAt是否有結束時間的值
			//假如有值則會判斷coupon.EndAt.Value是否有小於DateTime.UtcNow(目前的時間)
		{
			return CouponValidationResultDto.Failure("優惠券已過期");
		}
		// ☆最短租期
		if (coupon.MinRentalPeriod.HasValue && leaseDays < coupon.MinRentalPeriod.Value)
			//判斷租期是否符合最短租期要求
			//由coupon.MinRentalPeriod.HasValue判斷MinRentalPeriod是否有最小租期的值
			//假如有值則會判斷leaseDays是否有小於coupon.MinRentalPeriod.Value(最小租期的值)
			//leaseDays:租期天數,但不是Coupon可決定,需要從外部傳入
			//leaseDays = check_in - check_out
		{
			return CouponValidationResultDto.Failure($"租期不足{coupon.MinRentalPeriod.Value}天");
		}

		// ☆最長租期
		if (coupon.MaxRentalPeriod.HasValue && leaseDays > coupon.MaxRentalPeriod.Value)
			//判斷租期是否符合最長租期要求
			//由coupon.MaxRentalPeriod.HasValue判斷MaxRentalPeriod是否有最大租期的值
			//假如有值則會判斷leaseDays是否有小於coupon.MaxRentalPeriod.Value(最大租期的值)
			//leaseDays:租期天數,但不是Coupon可決定,需要從外部傳入
			//leaseDays = check_in - check_out
		{
			return CouponValidationResultDto.Failure($"租期超過{coupon.MaxRentalPeriod.Value}天");
		}
		// ☆百分比金額與固定金額合理性
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
		// ☆是否被刪除
		if (coupon.IsDeleted)
			//判斷優惠券是否被軟刪除
			//if (coupon.IsDeleted==true)就表示優惠券已被刪除
		{
			return CouponValidationResultDto.Failure("優惠券已失效");
		}
		// ☆最低金額
		if (coupon.LowSpend.HasValue && totalAmount < coupon.LowSpend.Value)
			//判斷消費金額是否符合最低消費要求
			//由coupon.LowSpend.HasValue判斷LowSpend是否有最低消費的值
			//假如有值則會判斷totalAmount是否有小於coupon.LowSpend.Value(最低消費的值)
			//totalAmount:消費金額,但不是Coupon可決定,需要從外部傳入
		{
			return CouponValidationResultDto.Failure($"價格不可低於{coupon.LowSpend.Value}元");
		}

		return CouponValidationResultDto.Success("成功使用");
	}

	private CouponValidationResultDto ValidateDate(Coupon coupon, DateTime? useDate, int leaseDays)
	{
		// ☆租期區間
		if (useDate.HasValue)
		{
			var rentalEndDate = useDate.Value.AddDays(leaseDays - 1);

			if (coupon.StartRentalPeriod.HasValue && useDate.Value < coupon.StartRentalPeriod.Value)
				return CouponValidationResultDto.Failure(
					$"優惠券尚未生效，使用日期需在 {coupon.StartRentalPeriod.Value:yyyy/MM/dd} 之後");

			if (coupon.EndRentalPeriod.HasValue && rentalEndDate > coupon.EndRentalPeriod.Value)
				return CouponValidationResultDto.Failure(
					$"優惠券已過期，租期結束日期需在 {coupon.EndRentalPeriod.Value:yyyy/MM/dd} 之前");
		}
		return CouponValidationResultDto.Success("成功使用");
	}

	// ☆地區限制
	private async Task<CouponValidationResultDto> ValidateDistrictAsync(Coupon coupon, int? cityId)
	{
		if (!cityId.HasValue) 
		{
			return CouponValidationResultDto.Success("未指定使用城市");
		}

		var couponCityMap = new Dictionary<string, int>
		{
			{ "TAIPEI", 1 },{ "NEWTAIPEI", 2 },{ "TAOYUAN", 3 },{ "TAICHUNG", 4 },
			{ "TAINAN", 5 },{ "KAOHSIUNG", 6 },{ "KEELUNG", 7 },{ "HSINCHU", 8 },
			{ "CHIAYI", 9 },{ "YILAN", 10 },{ "HSINCHUCOUNTY", 11 },{ "MIAOLI", 12 },
			{ "CHANGHUA", 13 },{ "NANTOU", 14 },{ "YUNLIN", 15 },{ "CHIAYICOUNTY", 16 },
			{ "PINTUNG", 17 },{ "TAITUNG", 18 },{ "HUALIEN", 19 },{ "PENGHU", 20 },
			{ "KINMEN", 21 },{ "LIANJIANG", 22 }
		};

		if (!couponCityMap.ContainsKey(coupon.DiscountCode)) 
		{
			return CouponValidationResultDto.Success("無地區限制");
		}

		var allowedCityId = couponCityMap[coupon.DiscountCode];
		if (allowedCityId != cityId.Value) 
		{
			return CouponValidationResultDto.Failure("此優惠券僅限指定城市使用");
		}
			return CouponValidationResultDto.Success("成功使用");

		//// ☆地區限制
		//if (districtId.HasValue) 
		//	//只有districtId有值時,才會進行判斷避免null
		//{
		//	var districts = await _districtrepo.Query()
		//		.Where(cd => cd.CouponId == coupon.CouponId)
		//		.Select(cd => cd.DistrictId)
		//		.ToListAsync();
		//	if (districts.Any() && !districts.Contains(districtId.Value))
		//	{
		//		return CouponValidationResultDto.Failure($"優惠券不是用於此地區");
		//	}
		//}
		//return CouponValidationResultDto.Success("成功使用");
	}

	private async Task<CouponValidationResultDto> ValidateNewUserAsync(Coupon coupon, int userId)
	{
		var bookingCount = await _bookingrepo.CountByUserId(userId);
		if (bookingCount > 0) 
		{
			return CouponValidationResultDto.Failure("此優惠券僅限新用戶使用");
		}
		return CouponValidationResultDto.Success("成功使用");
	}

	private async Task<CouponValidationResultDto> ValidateBirthdayAsync(Coupon coupon, int userId)
	{
		var user = await _userrepo.GetByIdAsync(userId);
		if (user == null) 
		{
			return CouponValidationResultDto.Failure("查無此用戶資料");
		}
		if (user.BirthDate.Month != DateTime.UtcNow.Month) 
		{
			return CouponValidationResultDto.Failure("此優惠券僅限生日月份使用");
		}
		return CouponValidationResultDto.Success("成功使用");
	}
}