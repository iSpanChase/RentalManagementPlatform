//Repository層 (查尋User資料的介面)
//用於servics判斷『新用戶』的商業邏輯
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repository.Interfaces
{
	public interface IBookingReadRepository
	{
		Task<int> CountByUserId(int guestId);
		IQueryable<Booking> Query();
		Task<Booking?> GetByIdAsync(int GuestId);//取得該使用者過去訂單數量(判斷是新用戶)
	}
}
