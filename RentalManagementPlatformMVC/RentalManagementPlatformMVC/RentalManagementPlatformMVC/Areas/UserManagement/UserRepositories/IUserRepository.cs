using RentalManagementPlatformMVC.Repositories;
using UserEntity = RentalManagementPlatformMVC.Models.User;

namespace RentalManagementPlatformMVC.Areas.UserManagement.UserRepositories
{
	/// <summary>
	/// 使用者資料介面。僅處理資料存取，不包含商業規則。
	/// </summary>
	public interface IUserRepository : IRepository<UserEntity>
	{
		/// <summary>
		/// 依帳號取得使用者（可能為 null）。
		/// </summary>
		Task<UserEntity?> GetByUsernameAsync(string username);

		/// <summary>
		/// 指定帳號是否已存在。
		/// </summary>
		Task<bool> ExistsByUsernameAsync(string username);

		/// <summary>
		/// 指定 Email 是否已存在。
		/// </summary>
		Task<bool> ExistsByEmailAsync(string email);

		/// <summary>
		/// 以不追蹤模式取得查詢（用於清單/查詢）。
		/// </summary>
		IQueryable<UserEntity> Query();
	}
}
