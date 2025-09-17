using RentalManagementPlatformMVC.CommonRepos;
using PermissionEntity = RentalManagementPlatformMVC.Models.Permission;

namespace RentalManagementPlatformMVC.Areas.Permissions.PermissionsRepositories
{
	public interface IPermissionsRepository : IRepository<PermissionEntity>
	{
		IQueryable<PermissionEntity> Query();
		Task<bool> ExistsByCodeAsync(string permCode, int? excludeId = null);
		Task<bool> ExistsByModuleActionAsync(string module, string action, int? excludeId = null);
	}
}
