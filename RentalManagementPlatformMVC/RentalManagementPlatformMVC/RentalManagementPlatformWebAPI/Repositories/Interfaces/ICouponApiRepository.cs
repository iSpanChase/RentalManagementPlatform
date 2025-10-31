//優惠券資料存取API服務介面
using RentalManagementPlatformWebAPI.DTOS;
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repository.Interfaces
{
	public interface ICouponApiRepository
	{
		//依據優惠券代碼取得優惠券資料
		//回傳對應的coupon物件,若無則回傳null
		Task<Coupon?>GetByCouponAsync(string code);

		//將指定的優惠券指派給特定使用者
		//通常在相關的促銷活動或使用者兌換優惠碼後,將優惠券與使用者關聯起來
		Task<bool>AssignCouponToUserAsync(string code,int userId);

		//將優惠券標記為已使用
		//通常在訂單完成後,將優惠券狀態更新為已使用,以防止重複使用
		Task<bool> MarkUsedAsync(int couponId,int userId);

		//取得所有優惠券列表
		Task<IEnumerable<Coupon>> GetAllCouponsAsync();
	}
}
