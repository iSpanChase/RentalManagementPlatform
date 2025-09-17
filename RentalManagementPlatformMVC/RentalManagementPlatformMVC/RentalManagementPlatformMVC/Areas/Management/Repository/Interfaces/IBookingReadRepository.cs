//Repository層 (查尋User資料的介面)
//用於servics判斷『新用戶』的商業邏輯
namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	public interface IBookingReadRepository
	{
		Task<int> CountByUserId(int guestId);
		IQueryable<Models.Booking> Query();
		Task<Models.Booking?> GetByIdAsync(int GuestId);//取得該使用者過去訂單數量(判斷是新用戶)
	}
}
