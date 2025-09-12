//Repository層 (查尋User資料的介面)
//用於servics判斷『生日』 的商業邏輯
namespace RentalManagementPlatformMVC.Areas.Management.Repository.Interfaces
{
	public interface IUserReadRepository
	{
		IQueryable<Models.User> Query();
		Task<Models.User?> GetByIdAsync(int UserId);//判斷用戶的生日時間
	}
}
