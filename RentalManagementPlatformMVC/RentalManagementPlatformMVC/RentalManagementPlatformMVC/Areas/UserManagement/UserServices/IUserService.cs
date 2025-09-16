using RentalManagementPlatformMVC.Areas.UserManagement.UserDTOs;
using RentalManagementPlatformMVC.Areas.UserManagement.ViewModels;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserServices
{
	/// <summary>
	/// MVC 層使用者服務（Facade）。負責 DTO 與 ViewModel 的轉換與轉呼叫 Application 服務。
	/// </summary>
	public interface IUserService
	{
		/// <summary>
		/// 取得使用者清單（支援查詢、排序與分頁）。
		/// </summary>
		/// <param name="keyword">關鍵字（可為 null）。</param>
		/// <param name="page">頁碼（1 起算）。</param>
		/// <param name="pageSize">每頁筆數。</param>
		/// <returns>清單與總筆數。</returns>
		Task<(IReadOnlyList<UserListItemDto> Items, int Total)> ListAsync(UserFilterVm filter);
		
		/// <summary>
		/// 取得使用者明細。
		/// </summary>
		/// <param name="userId">使用者主鍵。</param>
		/// <returns>使用者明細 DTO。</returns>
		Task<UserDetailDto> GetAsync(int userId);

		/// <summary>
		/// 建立使用者。
		/// </summary>
		/// <param name="dto">建立 DTO。</param>
		/// <returns>新建使用者的主鍵。</returns>
		Task<int> CreateAsync(CreateUserDto dto);

		/// <summary>
		/// 更新使用者。
		/// </summary>
		/// <param name="dto">更新 DTO。</param>
		Task UpdateAsync(UpdateUserDto dto);

		/// <summary>
		/// 刪除使用者。
		/// </summary>
		/// <param name="userId">使用者主鍵。</param>
		Task DeleteAsync(int userId);

		Task<AssignUserRolesVm> GetAssignRolesAsync(int userId);
		Task AssignRolesAsync(int userId, int[] roleIds);
	}
}
