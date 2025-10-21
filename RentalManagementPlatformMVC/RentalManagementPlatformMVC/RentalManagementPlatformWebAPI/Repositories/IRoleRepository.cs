using RentalManagementPlatformWebAPI.Models;

namespace RentalManagementPlatformWebAPI.Repositories
{
	/// <summary>
	/// 角色讀寫與指派/回收使用者的資料存取介面
	/// </summary>
	public interface IRoleRepository
	{
		/// <summary>以主鍵取得角色</summary>
		Task<Role?> GetByIdAsync(int roleId);

		/// <summary>以角色代碼(如 ADMIN/TENANT)取得角色</summary>
		Task<Role?> GetByCodeAsync(string roleCode);

		/// <summary>取得全部角色（依 RoleCode 排序）</summary>
		Task<List<Role>> GetAllAsync();

		/// <summary>把指定使用者指派到角色（若已存在則忽略）</summary>
		Task AssignUserAsync(int roleId, int userId);

		/// <summary>把指定使用者從角色移除（若不存在則忽略）</summary>
		Task RevokeUserAsync(int roleId, int userId);

		/// <summary>該角色是否仍被任何使用者使用（用於刪除前檢查）</summary>
		Task<bool> HasUsersAsync(int roleId);
	}
}
