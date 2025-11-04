//Repository層 (查尋User資料的介面)
//用於servics判斷『生日』 的商業邏輯
using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repository.Interfaces
{
	public interface IUserReadRepository
	{
		IQueryable<User> Query();//回傳 User 資料表的 延遲查詢
		Task<User?> GetByIdAsync(int UserId);//非同步取得單一用戶資料
	}
}
