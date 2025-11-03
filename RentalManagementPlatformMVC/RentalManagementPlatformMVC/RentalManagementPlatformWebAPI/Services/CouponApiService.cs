
using RentalManagementPlatformWebAPI.DTOS;
using RentalManagementPlatformWebAPI.Models;
using RentalManagementPlatformWebAPI.Repositories.Interfaces;
using RentalManagementPlatformWebAPI.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using RentalManagementPlatformWebAPI.Repository.Interfaces;

//優惠券驗證、查詢、領取標記、兌換列表等功能的實作

namespace RentalManagementPlatformWebAPI.Services
{
    public class CouponApiService : ICouponApiService
    {
        private readonly ICouponDbRepository _couponRepository;
        private readonly IBookingReadRepository _bookingRepository;
        private readonly IUserReadRepository _userRepository;
        private readonly ILogger<CouponApiService> _logger;
        private readonly RentalManagementPlatformSqlContext _context;

        public CouponApiService(
            ICouponDbRepository couponRepository,
            IBookingReadRepository bookingRepository,
            IUserReadRepository userRepository,
            ILogger<CouponApiService> logger,
            RentalManagementPlatformSqlContext context)
        {
            _couponRepository = couponRepository;
            _bookingRepository = bookingRepository;
            _userRepository = userRepository;
            _logger = logger;
            _context = context;
        }

        //回傳優惠券的驗證結果(是否有效/折扣金額/最終價格/錯誤代碼)
        public async Task<CouponValidationResponseDto> ValidateCouponAsync(CouponValidationRequestDto request)
        {
            var coupon = await _couponRepository.GetCouponByCodeAsync(request.DiscountCode);

            // ☆ 1. 基本驗證 (存在、刪除、過期、低消)
            if (coupon == null || coupon.IsDeleted)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_NOT_FOUND", Message = "優惠券不存在或已失效。" };
            }

            if (coupon.EndAt.HasValue && DateTime.UtcNow > coupon.EndAt.Value)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_EXPIRED", Message = "此優惠券已過期。" };
            }

            if (coupon.LowSpend.HasValue && request.TotalAmount < coupon.LowSpend.Value)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_MIN_SPEND", Message = $"訂單金額需滿 {coupon.LowSpend} 元方可使用。" };
            }

            // ☆ 2. 租期長度驗證
            if (coupon.MinRentalPeriod.HasValue && request.LeaseDays < coupon.MinRentalPeriod.Value)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_LEASE_DAYS_MIN", Message = $"租期需至少 {coupon.MinRentalPeriod.Value} 天。" };
            }
            if (coupon.MaxRentalPeriod.HasValue && request.LeaseDays > coupon.MaxRentalPeriod.Value)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_LEASE_DAYS_MAX", Message = $"租期不可超過 {coupon.MaxRentalPeriod.Value} 天。" };
            }

            // ☆ 3. 租期區間驗證
            if (request.UseDate.HasValue)
            {
                var rentalEndDate = request.UseDate.Value.AddDays(request.LeaseDays - 1);
                if (coupon.StartRentalPeriod.HasValue && request.UseDate.Value < coupon.StartRentalPeriod.Value)
                {
                    return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_RENTAL_PERIOD_START", Message = $"優惠券尚未生效，需在 {coupon.StartRentalPeriod.Value:yyyy/MM/dd} 後使用。" };
                }
                if (coupon.EndRentalPeriod.HasValue && rentalEndDate > coupon.EndRentalPeriod.Value)
                {
                    return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_RENTAL_PERIOD_END", Message = $"租期結束日已超過優惠券期限 {coupon.EndRentalPeriod.Value:yyyy/MM/dd}。" };
                }
            }

            // ☆ 4. 折扣額度合理性驗證
            if (coupon.DiscountMethod == "percentage" && (coupon.DiscountQuota <= 0 || coupon.DiscountQuota > 100))
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_INVALID_QUOTA", Message = "優惠券折扣額度設定不合理。" };
            }
            if (coupon.DiscountMethod == "amount" && coupon.DiscountQuota <= 0)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_INVALID_QUOTA", Message = "優惠券折扣額度設定不合理。" };
            }

            // ☆ 5. 地區限制驗證 (硬編碼邏輯)
            var districtResult = ValidateDistrict(coupon, request.CityId);
            if (!districtResult.IsValid) return districtResult;

            // ☆ 6. 特殊用戶類型驗證 (硬編碼邏輯)
            if (request.UserId.HasValue)
            {
                if (coupon.DiscountCode.StartsWith("WELCOME"))
                {
                    var newUserResult = await ValidateNewUserAsync(request.UserId.Value);
                    if (!newUserResult.IsValid) return newUserResult;
                }
                if (coupon.DiscountCode.StartsWith("BDAY"))
                {
                    var birthdayResult = await ValidateBirthdayAsync(request.UserId.Value);
                    if (!birthdayResult.IsValid) return birthdayResult;
                }
            }

            // ☆ 7. 計算折扣金額
            decimal discountAmount = 0;
            if (coupon.DiscountMethod == "amount")
            {
                discountAmount = coupon.DiscountQuota ?? 0;
            }
            else if (coupon.DiscountMethod == "percentage")
            {
                discountAmount = request.TotalAmount * ((100m - (coupon.DiscountQuota ?? 0)) / 100m);
            }

            // 確保折扣金額不超過總金額
            if (discountAmount > request.TotalAmount)
            {
                discountAmount = request.TotalAmount;
            }

            // ☆ 8. 回傳成功結果
            return new CouponValidationResponseDto
            {
                IsValid = true,
                Message = "優惠券可使用",
                DiscountAmount = Math.Round(discountAmount),
                FinalPrice = request.TotalAmount - Math.Round(discountAmount)
            };
        }

        public async Task<CouponValidationResponseDto> ValidateCouponRawAsync(string discountCode, decimal totalAmount, int leaseDays, int? userId)
        {
            var validationRequest = new CouponValidationRequestDto
            {
                DiscountCode = discountCode,
                TotalAmount = totalAmount,
                LeaseDays = leaseDays,
                UserId = userId
            };
            return await ValidateCouponAsync(validationRequest);
        }

        #region Private Validation Helpers

        private CouponValidationResponseDto ValidateDistrict(Coupon coupon, int? cityId)
        {
            // 此處保留硬編碼的地區驗證邏輯
            var couponCityMap = new Dictionary<string, int>
            {
                { "TAIPEI", 1 },{ "NEWTAIPEI", 2 },{ "TAOYUAN", 3 },{ "TAICHUNG", 4 },
                { "TAINAN", 5 },{ "KAOHSIUNG", 6 },{ "KEELUNG", 7 },{ "HSINCHU", 8 },
                { "CHIAYI", 9 },{ "YILAN", 10 },{ "HSINCHUCOUNTY", 11 },{ "MIAOLI", 12 },
                { "CHANGHUA", 13 },{ "NANTOU", 14 },{ "YUNLIN", 15 },{ "CHIAYICOUNTY", 16 },
                { "PINTUNG", 17 },{ "TAITUNG", 18 },{ "HUALIEN", 19 },{ "PENGHU", 20 },
                { "KINMEN", 21 },{ "LIANJIANG", 22 }
            };

            // 檢查優惠碼是否為地區碼之一
            var relevantCode = coupon.DiscountCode.Split('_').FirstOrDefault(); // 例如 "TAIPEI_2024" -> "TAIPEI"
            if (relevantCode != null && couponCityMap.ContainsKey(relevantCode))
            {
                if (!cityId.HasValue)
                {
                    return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_CITY_REQUIRED", Message = "此優惠券需要提供城市資訊。" };
                }
                if (couponCityMap[relevantCode] != cityId.Value)
                {
                    return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_CITY_MISMATCH", Message = "此優惠券不適用於您選擇的地區。" };
                }
            }
            return new CouponValidationResponseDto { IsValid = true }; // 無地區限制或符合地區限制
        }

        private async Task<CouponValidationResponseDto> ValidateNewUserAsync(int userId)
        {
            var bookingCount = await _bookingRepository.CountByUserId(userId);
            if (bookingCount > 0)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_NOT_NEW_USER", Message = "此優惠券僅限新用戶使用。" };
            }
            return new CouponValidationResponseDto { IsValid = true };
        }

        private async Task<CouponValidationResponseDto> ValidateBirthdayAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_USER_NOT_FOUND", Message = "查無此用戶資料。" };
            }
            if (user.BirthDate.Month != DateTime.UtcNow.Month)
            {
                return new CouponValidationResponseDto { IsValid = false, ErrorCode = "ERR_NOT_BIRTHDAY_MONTH", Message = "此優惠券僅限生日當月使用。" };
            }
            return new CouponValidationResponseDto { IsValid = true };
        }

        #endregion

		// 查詢使用者的優惠券列表
		public async Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(int userId)
        {
            var userCoupons = await _couponRepository.GetUserCouponsByGuestIdAsync(userId);

                return userCoupons.Select(cg => new UserCouponDto
                {
                    CouponId = cg.Coupon.CouponId,
                    CouponName = cg.Coupon.CouponName,
                    Description = cg.Coupon.Description,
                    DiscountCode = cg.Coupon.DiscountCode,
                    Status = DetermineCouponStatus(cg),
                    EndAt = cg.Coupon.EndAt ?? DateTime.MaxValue,
                    StartAt = cg.Coupon.StartRentalPeriod ?? DateTime.MinValue, // 假設 StartRentalPeriod 是開始時間
                    DiscountMethod = cg.Coupon.DiscountMethod,
                    DiscountQuota = cg.Coupon.DiscountQuota ?? 0,
                    LowSpend = cg.Coupon.LowSpend
                });        }

        private string DetermineCouponStatus(CouponGuest couponGuest)
        {
            _logger.LogInformation("Determining status for CouponGuestId: {CouponGuestId}, RemoveAt: {RemoveAt}, CouponEndAt: {CouponEndAt}, CurrentTime: {CurrentTime}", 
                couponGuest.CouponGuestId, couponGuest.RemoveAt, couponGuest.Coupon.EndAt, DateTime.UtcNow);

            if (couponGuest.RemoveAt.HasValue) 
            {
                _logger.LogInformation("Status: used (RemoveAt has value)");
                return "used";
            }
            if (couponGuest.Coupon.EndAt < DateTime.UtcNow) 
            {
                _logger.LogInformation("Status: expired (CouponEndAt is in the past)");
                return "expired";
            }
            _logger.LogInformation("Status: 可使用");
            return "可使用";
        }

        public async Task<bool> RedeemPromoCodeAsync(RedeemPromoCodeRequestDto request)
        {
			// 查詢優惠券
			var coupon = await _couponRepository.GetCouponByCodeAsync(request.DiscountCode);
            if (coupon == null) return false; // 券不存在

            // 檢查使用者是否已領取
            var existingLink = await _couponRepository.GetUserCouponAsync(request.UserId, coupon.CouponId);
            if (existingLink != null) return false; // 已領取

            // TODO: 檢查總量限制等其他業務邏輯

            var newCouponGuestId = await _context.CouponGuests.AnyAsync() ?
                                   await _context.CouponGuests.MaxAsync(cg => cg.CouponGuestId) + 1 :
                                   1;

            var newCouponGuest = new CouponGuest
            {
                CouponGuestId = newCouponGuestId,
                GuestId = request.UserId,
                CouponId = coupon.CouponId,
                CreateAt = DateTime.UtcNow
            };

            _logger.LogInformation("Attempting to add new CouponGuest: GuestId={GuestId}, CouponId={CouponId}, CreateAt={CreateAt}", 
                newCouponGuest.GuestId, newCouponGuest.CouponId, newCouponGuest.CreateAt);

            try
            {
                await _couponRepository.AddUserCouponAsync(newCouponGuest);
                await _couponRepository.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error redeeming promo code for UserId: {UserId}, DiscountCode: {DiscountCode}", request.UserId, request.DiscountCode);
                return false;
            }
        }

        public async Task<bool> MarkUsedAsync(MarkUsedRequestDto request)
        {
            var userCoupon = await _couponRepository.GetUserCouponAsync(request.UserId, request.CouponId);
            if (userCoupon == null) return false; // 使用者並未擁有此券

            // 將此券標記為已使用，設定 RemoveAt 為當前時間
            userCoupon.RemoveAt = DateTime.UtcNow;
            // 如果有 BookingId 欄位，也可以在這裡設定 userCoupon.BookingId = request.BookingId;
            _context.Update(userCoupon); // 顯式調用 Update 以確保 EF Core 追蹤到變更

            await _couponRepository.SaveChangesAsync();

            return true;
        }

        public async Task<IEnumerable<CouponDto>> GetAllCouponsAsync()
        {
            var coupons = await _couponRepository.GetAllAsync();
            return coupons.Select(c => new CouponDto
            {
                CouponId = c.CouponId,
                CouponName = c.CouponName,
                Description = c.Description,
                DiscountCode = c.DiscountCode,
                EndAt = c.EndAt,
                StartRentalPeriod = c.StartRentalPeriod,
                DiscountMethod = c.DiscountMethod,
                DiscountQuota = c.DiscountQuota,
                LowSpend = c.LowSpend
            });
        }
    }
}
