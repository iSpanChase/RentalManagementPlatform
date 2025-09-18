using RentalManagementPlatformMVC.CommonRepos;
using RolesEntity = RentalManagementPlatformMVC.Models.Role;

namespace RentalManagementPlatformMVC.Areas.Roles.RolesRepositories
{
	/// <summary>
	/// 角色資料介面。僅處理資料存取，不包含商業規則。
	/// </summary>
	public interface IRolesRepository : IRepository<RolesEntity>
	{
		/// <summary>
		/// 依角色 ID 取得角色（可能為 null）。
		/// </summary>
		/// <param name="roleId">角色 ID。</param>
		/// <returns>角色（可能為 null）。</returns>
		Task<RolesEntity?> GetByRoleNameAsync(string roleName);
		/// <summary>
		/// 依會員 ID 取得角色（可能為 null）。
		/// </summary>
		/// <param name="roleId"></param>
		/// <returns></returns>
		Task<List<int>> GetUserIdsInRoleAsync(int roleId);
		Task<List<int>> GetPermissionIdsInRoleAsync(int roleId);

		/// <summary>
		/// 指定角色是否已存在。
		/// </summary>
		Task<bool> ExistsByRoleNameAsync(string roleName);
		/// <summary>
		/// 指定描述是否已存在。
		/// </summary>
		Task<bool> ExistsByDescriptionAsync(string description);

		Task<bool> ExistsByRoleCodeAsync(string roleCode);
		/// <summary>
		/// 以不追蹤模式取得查詢（用於清單/查詢）。
		/// </summary>
		IQueryable<RolesEntity> Query();
	}
}
