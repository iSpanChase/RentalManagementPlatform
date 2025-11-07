using RentalManagementPlatformWebAPI.DTOS;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RentalManagementPlatformWebAPI.Services.Interfaces
{
	public interface ICouponApiService
	{
		/// <summary>
		/// 驗證優惠券的有效性，並回傳包含折扣金額的詳細結果。
		/// </summary>
		Task<CouponValidationResponseDto> ValidateCouponAsync(CouponValidationRequestDto request);

		/// <summary>
		/// 讓使用者領取一張公開的優惠券到個人帳戶。
		/// </summary>
		Task<bool> RedeemPromoCodeAsync(RedeemPromoCodeRequestDto request);

		/// <summary>
		/// 在訂單成立時，核銷一張優惠券並將其標記為已使用。
		/// </summary>
		Task<bool> MarkUsedAsync(MarkUsedRequestDto request);

		/// <summary>
		/// 取得所有公開可用的優惠券列表。
		/// </summary>
		Task<IEnumerable<CouponDto>> GetAllCouponsAsync();

		/// <summary>
		/// 取得特定使用者所擁有的所有優惠券 (包含狀態)。
		/// </summary>
		Task<IEnumerable<UserCouponDto>> GetUserCouponsAsync(int userId);
	}
}